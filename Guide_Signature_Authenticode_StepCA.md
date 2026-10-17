# 🛡️ Guide : Signature de Binaires Windows avec step-ca

## 1. Séparation des rôles (PKI)

| Rôle | Certificat | Usage (EKU) | Emplacement de la clé privée |
| :--- | :--- | :--- | :--- |
| **Logiciel (.exe)** | `WorldPinball Publisher` | **Code Signing** | Poste de build / CI-CD *(JAMAIS sur les NUCs)* |
| **Machine (NUC)** | `NUC-Flipper-042` | **Client Auth (mTLS)** | Dans le **TPM 2.0** de chaque NUC physique |
| **Backend (API)** | `api.worldpinball.com` | **Server Auth** | Reverse Proxy / Kestrel |

> [!important] Règle d'or
> On ne signe **jamais** avec la Root CA. La Root CA signe l'Intermediate CA, et l'Intermediate CA émet le certificat de signature.

---

## 2. Génération du certificat dans step-ca

### Étape 1 : Créer le template et générer le certificat
À exécuter dans ton terminal PowerShell hôte :

```powershell
docker exec worldpinball-step-ca sh -c 'cat << "EOF" > /tmp/codesigning.tpl
{
  "subject": {
    "organization": "WorldPinball",
    "commonName": "{{ .Subject.CommonName }}"
  },
  "keyUsage": ["digitalSignature"],
  "extKeyUsage": ["codeSigning"],
  "basicConstraints": {
    "isCA": false
  }
}
EOF'
```

Puis générer la clé et le certificat :

```powershell
docker exec worldpinball-step-ca step certificate create "WorldPinball Software Publisher" /tmp/codesign.crt /tmp/codesign.key --template /tmp/codesigning.tpl --ca /home/step/certs/intermediate_ca.crt --ca-key /home/step/secrets/intermediate_ca_key --ca-password-file /home/step/secrets/password --no-password --insecure --force
```

### Étape 2 : Créer le fichier .pfx pour Windows

```powershell
docker exec worldpinball-step-ca step certificate p12 /tmp/codesign.pfx /tmp/codesign.crt /tmp/codesign.key --ca /home/step/certs/intermediate_ca.crt --password-file /home/step/secrets/password --force
```

### Étape 3 : Récupérer les fichiers sur ta machine

```powershell
docker cp worldpinball-step-ca:/tmp/codesign.pfx ./codesign.pfx
docker cp worldpinball-step-ca:/home/step/certs/root_ca.crt ./root_ca.crt
```

---

## 3. Installation et Signature

### Étape 4 : Déclarer la Root CA sur le NUC (1 seule fois)
À lancer en **PowerShell Administrateur** sur le NUC :

```powershell
Import-Certificate -FilePath .\root_ca.crt -CertStoreLocation Cert:\LocalMachine\Root
```

### Étape 5 : Signer l'exécutable avec signtool
À lancer dans le *Developer Command Prompt* ou avec le Windows SDK :

```powershell
signtool sign /fd SHA256 /f .\codesign.pfx /p "WorldPinballSecretPass123!" /tr http://timestamp.digicert.com /td SHA256 "C:\Chemin\Vers\WorldPinball.exe"
```

> [!tip] À quoi servent les options ?
> - `/fd SHA256` : Hachage sécurisé du binaire.
> - `/tr ... /td SHA256` : Horodatage RFC 3161 (le binaire reste valide même quand le certificat expire).

---

## 4. Vérification

### En ligne de commande
```powershell
Get-AuthenticodeSignature .\WorldPinball.exe
```

### Dans Windows
1. Clic droit sur `WorldPinball.exe` > **Propriétés**
2. Onglet **Signatures numériques**
3. Sélectionner `WorldPinball Software Publisher` > **Détails**
4. Statut attendu : *"Cette signature numérique est OK"*
