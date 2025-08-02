variable "project_name" {
  description = "A unique name for the project to prefix resources."
  type        = string
  default     = "nexusphere-mvp"
}

variable "aws_region" {
  description = "The AWS region to deploy resources into."
  type        = string
  default     = "af-south-1"
}

variable "vpc_cidr" {
  description = "The CIDR block for the VPC."
  type        = string
  default     = "10.0.0.0/16"
}

variable "availability_zones" {
  description = "A list of Availability Zones to use for the VPC."
  type        = list(string)
  default     = ["af-south-1a", "af-south-1b"]
}

variable "app_container_port" {
  description = "The port the C# application listens on inside the container."
  type        = number
  default     = 8080
}

variable "ecr_repo_name" {
  type = string
  default = "team9-repository"
}

variable "ecs_cluster_name" {
  type = string
  default = "team9_ecs_cluster"
}

variable "domain_name" {
  description = "The root custom domain name for the application."
  type = string
  default     = "peerspace.online"
}

variable "internal_api_port" {
  description = "The port for the internal ALB listener for service-to-service communication."
  type        = number
  default     = 8081
}