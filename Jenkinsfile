// Declarative Pipeline for WorldPinball (.NET 10 Solution)
pipeline {
    agent any

    environment {
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
        BUILD_CONFIGURATION = 'Release'
        SOLUTION_NAME = 'Leaderboard_WorldPinball.sln'
        TEST_RESULTS_DIR = 'TestResults'
    }

    options {
        // Terminate build if it exceeds 30 minutes
        timeout(time: 30, unit: 'MINUTES')
        // Retain only the last 10 builds to conserve agent disk space
        buildDiscarder(logRotator(numToKeepStr: '10'))
        // Prepend timestamps to all console logs
        timestamps()
        // Enable ANSI color codes in console output
        ansiColor('xterm')
    }

    stages {
        stage('Checkout') {
            steps {
                echo 'Checking out source code from Git repository...'
                checkout scm
            }
        }

        stage('Restore Dependencies') {
            steps {
                echo 'Restoring NuGet packages across solution...'
                sh "dotnet restore ${SOLUTION_NAME}"
            }
        }

        stage('Build Solution') {
            steps {
                echo "Compiling solution in ${BUILD_CONFIGURATION} mode..."
                sh "dotnet build ${SOLUTION_NAME} --configuration ${BUILD_CONFIGURATION} --no-restore"
            }
        }

        stage('Execute Tests & Code Coverage') {
            steps {
                echo 'Running unit and integration tests with TRX logging and coverage...'
                sh """
                    dotnet test ${SOLUTION_NAME} \
                        --configuration ${BUILD_CONFIGURATION} \
                        --no-build \
                        --logger "trx;LogFileName=test_results.trx" \
                        --results-directory "${TEST_RESULTS_DIR}" \
                        --collect:"XPlat Code Coverage"
                """
            }
            post {
                always {
                    // Ingest TRX reports into Jenkins test result trend charts
                    mstest testResultsFile: "${TEST_RESULTS_DIR}/*.trx", keepLongStdio: true
                }
            }
        }

        stage('SonarQube Static Analysis') {
            // Executed when SonarQube environment/credentials are configured on the agent
            when {
                expression { return env.SONAR_TOKEN != null }
            }
            steps {
                echo 'Executing SonarQube static analysis and waiting for Quality Gate...'
                withSonarQubeEnv('SonarQube-Server') {
                    sh """
                        dotnet sonarscanner begin /k:"worldpinball" \
                            /d:sonar.cs.opencover.reportsPaths="${TEST_RESULTS_DIR}/**/coverage.opencover.xml"
                        dotnet build ${SOLUTION_NAME} --configuration ${BUILD_CONFIGURATION} --no-restore
                        dotnet sonarscanner end
                    """
                }
                // Halt the pipeline if the Quality Gate fails (e.g. coverage dropped below threshold)
                timeout(time: 5, unit: 'MINUTES') {
                    waitForQualityGate abortPipeline: true
                }
            }
        }
    }

    post {
        success {
            echo "CI pipeline completed successfully! All tests and quality gates passed."
        }
        failure {
            echo "CI pipeline failed! Review logs, failed tests, and Quality Gate status."
        }
        always {
            echo "Pipeline execution finished."
        }
    }
}
