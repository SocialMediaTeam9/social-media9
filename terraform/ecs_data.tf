data "aws_secretsmanager_secret" "google_secrets" {
  name = "team9-google"
}

data "aws_secretsmanager_secret" "jwt_secrets" {
  name = "team9-jwt-secret"
}