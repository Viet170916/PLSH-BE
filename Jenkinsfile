pipeline {
    agent { label 'agent_server' }

    environment {
        DOCKERHUB_CREDENTIALS = credentials('dockerhub-credentials')
        SONAR_SERVER = credentials('sonarqube-server-url')
        SONAR_TOKEN = credentials('g67_se490_spr25')
        STAGING_SERVER = 'http://192.168.230.101:5000'
        SNYK_TOKEN = credentials('snyk-api-token')
    }

    stages {
        stage('Info') {
            steps {
                sh 'whoami; pwd; ls'
            }
        }

        stage('Build and Publish .NET') {
            steps {
                dir('PLSH-BE') {
                    sh '''
                        dotnet clean PLSH-BE.sln
                        dotnet restore PLSH-BE.sln
                        dotnet build PLSH-BE.sln -c Release
                        dotnet publish API/API.csproj -c Release -o ./publish --self-contained false
                    '''
                }
            }
        }

        stage('SonarQube Scan') {
            steps {
                dir('PLSH-BE') {
                    script {
                        withSonarQubeEnv('Sonarqube server connection') {
                            sh '''
                                dotnet tool install --global dotnet-sonarscanner
                                export PATH="$PATH:/root/.dotnet/tools"
                                dotnet sonarscanner begin \
                                    /k:"plsh-be" \
                                    /d:sonar.host.url=$SONAR_SERVER \
                                    /d:sonar.login=$SONAR_TOKEN
                                dotnet build PLSH-BE.sln
                                dotnet sonarscanner end /d:sonar.login=$SONAR_TOKEN
                            '''
                        }

                        sleep 30 // Đợi SonarQube xử lý kết quả

                        def timestamp = new Date().format("yyyyMMdd_HHmmss")
                        env.TIMESTAMP = timestamp

                        sh """
                            curl -u ${SONAR_TOKEN}: ${SONAR_SERVER}/api/issues/search?componentKeys=plsh-be&impactSeverities=BLOCKER,HIGH,MEDIUM&statuses=OPEN,CONFIRMED -o issues_${timestamp}.json
                            python3 convert_issue_json.py issues_${timestamp}.json sonarqube-report-${timestamp}.html
                        """

                        archiveArtifacts artifacts: "sonarqube-report-${timestamp}.html", fingerprint: true
                    }
                }
            }
        }


        stage('Snyk Scan') {
            steps {
                dir('PLSH-BE') {
                    script {
                        sh 'npm install -g snyk'

                        sh 'snyk config set api=$SNYK_TOKEN'

                        def timestamp = new Date().format("yyyyMMdd_HHmmss")
                        env.TIMESTAMP = timestamp

                        sh """
                            snyk test --file=API/API.csproj --severity-threshold=high --json-file-output=snyk.json || true
                            [ -f snyk.json ] && snyk-to-html -i snyk.json -o snyk-report-${timestamp}.html || true
                        """

                        archiveArtifacts artifacts: "snyk-report-${timestamp}.html", fingerprint: true
                    }
                }
            }
        }


        stage('Build Docker Image') {
            steps {
                dir('PLSH-BE') {
                    sh '''
                        docker build -t plsh-be -f API/Dockerfile .
                        docker tag plsh-be co0bridae/plsh-be:latest
                    '''
                }
            }
        }

        stage('Trivy Scan') {
            steps {
                script {
                    def timestamp = new Date().format("yyyyMMdd_HHmmss")
                    env.TIMESTAMP = timestamp

                    sh """
                        trivy image --timeout 10m --format json --output plsh-be-trivy-${timestamp}.json --severity HIGH,CRITICAL plsh-be
                        python3 convert_json.py plsh-be-trivy-${timestamp}.json plsh-be-trivy-${timestamp}.html
                    """
                    archiveArtifacts artifacts: "plsh-be-trivy-${timestamp}.html", fingerprint: true
                }
            }
        }

        stage('Push Docker Image') {
            steps {
                sh '''
                    echo $DOCKERHUB_CREDENTIALS_PSW | docker login -u $DOCKERHUB_CREDENTIALS_USR --password-stdin
                    docker push co0bridae/plsh-be:latest
                '''
            }
        }

        
    }
}
