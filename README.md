# Chillde - On-Demand Handmade Creations Platform

## Introduction

A platform that connects customers with artisans, allowing users to order custom-made products on demand, such as decorative items, jewelry, handmade clothing, or personalized gifts. Artists can open a shop that offers a variety of customization services for customers to choose from. After selecting the appropriate service, users can submit a design, request, or specific description of the product they want, and artisans will create the products on demand.

## Prerequisites

Before you begin, ensure you have met the following requirements:

- .NET 9 or highers
- PostgreSQL
- Redis
- Cloudinary

## Getting Started

- Set up your `appsettings.json` inside the API project as following:

```
{
  // Your other settings
  "ConnectionStrings": {
    "LocalDb": "",
    "DeployDb": ""
  },
  "Redis": {
    "Configuration": "",
    "IsEnabled": "true"
  },
  "Cloudinary": {
    "Cloud": "",
    "ApiKey": "",
    "ApiSecret": "",
    "URL": ""
  },
  "URL": {
    "Client": "",
    "Server": ""
  },
  "JWT": {
    "ValidIssuer": "",
    "ValidAudience": "",
    "Secret": ""
  },
  "EmailSettings": {
    "Host": "",
    "Port": ,
    "DisplayName": "",
    "From": "",
    "Password": ""
  },
  "OAuth2": {
    "Google": {
      "ClientId": "",
      "ClientSecret": ""
    }
  }
}
```

- **Add Migration** if needed, then **Update Database** through **Entity Framework Core** and finally you can run the project.

## References

Updating...
