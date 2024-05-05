variable "location" {
  type        = string
  default     = "North Europe"
  description = "resource group location"
}

variable "resource_group_name" {
  type        = string
  default     = "rg-DiscussionForum"
  description = "resource group name"
}

variable "solution_name" {
  type        = string
  description = "solution name"
}

variable "image_tag" {
  type        = string
  description = "web app image tag"
}
