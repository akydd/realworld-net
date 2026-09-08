variable "project" {
  description = "Short lowercase name used as a prefix for resource names."
  type        = string
  default     = "realworld-net"
}

variable "location" {
  description = "Azure region."
  type        = string
  default     = "eastus"
}

variable "sql_admin_login" {
  description = "Azure SQL server administrator login."
  type        = string
  default     = "sqladmin"
}

variable "sql_admin_password" {
  description = "Azure SQL server administrator password. Provide via TF_VAR_sql_admin_password or a gitignored .tfvars — never commit it."
  type        = string
  sensitive   = true
}

variable "jwt_secret" {
  description = "HS256 signing key for JWTs (>= 32 chars). Provide via TF_VAR_jwt_secret or a gitignored .tfvars."
  type        = string
  sensitive   = true
}

variable "aspnetcore_environment" {
  description = "ASPNETCORE_ENVIRONMENT for the deployed app. See the note in the README about exposing the API docs."
  type        = string
  default     = "Production"
}

variable "client_ip" {
  description = "Your public IP, added to the SQL firewall so you can run EF migrations from your machine. Leave empty to skip."
  type        = string
  default     = ""
}
