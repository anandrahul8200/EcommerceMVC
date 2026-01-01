# E-commerce Management System - .NET Framework 4.8 MVC

A traditional ASP.NET MVC application built with .NET Framework 4.8 for managing an e-commerce system. This application serves as a baseline for demonstrating modernization to .NET Core with React/Angular using AWS Porting Assistant.

## Project Overview

This is a **Proof of Concept (PoC)** application that demonstrates:
- Traditional .NET Framework MVC architecture
- Entity Framework 6 for data access
- SQL Server database integration
- Server-side rendering with Razor views
- CRUD operations for Customers, Products, and Orders

## Technology Stack

- **.NET Framework 4.8**
- **ASP.NET MVC 5.2.9**
- **Entity Framework 6.4.4**
- **SQL Server** (EcommerceDB)
- **Bootstrap 5** (for UI)
- **Razor View Engine**

## Project Structure

```
EcommerceMVC/
├── Controllers/           # MVC Controllers
│   ├── HomeController.cs
│   ├── CustomersController.cs
│   ├── ProductsController.cs
│   └── OrdersController.cs
├── Models/               # Data models and EF context
│   ├── Customer.cs
│   ├── Product.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   ├── Category.cs
│   └── EcommerceContext.cs
├── Views/                # Razor views
│   ├── Home/
│   ├── Customers/
│   ├── Products/
│   ├── Orders/
│   └── Shared/
│       └── _Layout.cshtml
├── App_Start/            # Configuration
│   └── RouteConfig.cs
├── Web.config            # Application configuration
└── Global.asax.cs        # Application startup
```

## Features

### Dashboard (Home)
- Overview statistics (total customers, products, orders)
- Quick action buttons
- Navigation to all modules

### Customer Management
- View all customers
- Create new customers
- Edit customer details
- Delete customers
- Customer types: Regular, Premium, VIP
- Loyalty points tracking

### Product Management
- View all products with categories
- Create new products
- Edit product details
- Delete products
- Price and inventory tracking
- Featured products

### Order Management
- View all orders
- Order details with line items
- Order status tracking
- Payment status
- Customer information

## Database Schema

The application uses the following main tables:
- **Customers** - Customer information
- **Products** - Product catalog
- **Categories** - Product categories
- **Orders** - Order headers
- **OrderItems** - Order line items
- **Addresses** - Customer addresses
- **Inventory** - Stock management
- **Payments** - Payment transactions
- **Shipments** - Shipping information
- **Reviews** - Product reviews

## Getting Started

### Prerequisites

1. Windows Server 2019 or Windows 10/11
2. .NET Framework 4.8
3. SQL Server (2016 or later)
4. Visual Studio 2022 (or VS Code)
5. IIS (for deployment)

### Installation Steps

1. **Clone or download this project**

2. **Setup the database:**
   - Open SQL Server Management Studio
   - Run `ecommerce_schema.sql` to create the database
   - Run `ecommerce_test_data.sql` to load sample data

3. **Configure connection string:**
   - Open `Web.config`
   - Update the connection string with your SQL Server details:
   ```xml
   <add name="EcommerceDB" 
        connectionString="Data Source=YOUR_SERVER;Initial Catalog=EcommerceDB;Integrated Security=True;MultipleActiveResultSets=True" 
        providerName="System.Data.SqlClient" />
   ```

4. **Open in Visual Studio:**
   - Open `EcommerceMVC.csproj`
   - Restore NuGet packages (right-click solution > Restore NuGet Packages)
   - Build the solution (Ctrl+Shift+B)

5. **Run locally:**
   - Press F5 to run with debugging
   - Application will open in your default browser
   - Default URL: `https://localhost:44300`

6. **Deploy to IIS:**
   - See `DEPLOYMENT_GUIDE.md` for detailed deployment instructions

## Configuration

### Web.config Settings

**Connection String:**
```xml
<connectionStrings>
  <add name="EcommerceDB" 
       connectionString="..." 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

**App Settings:**
```xml
<appSettings>
  <add key="webpages:Version" value="3.0.0.0" />
  <add key="ClientValidationEnabled" value="true" />
  <add key="UnobtrusiveJavaScriptEnabled" value="true" />
</appSettings>
```

## Development Notes

### Entity Framework Configuration

- **Database-First approach** using Code-First models
- **Lazy Loading**: Disabled for performance
- **Proxy Creation**: Disabled
- **Connection**: Uses connection string from Web.config

### MVC Routing

Default route pattern:
```
{controller}/{action}/{id}
```

Examples:
- `/` → Home/Index
- `/Customers` → Customers/Index
- `/Products/Details/5` → Products/Details with id=5

## Modernization Path

This application is designed to be modernized using:

1. **AWS Porting Assistant for .NET**
   - Analyzes compatibility with .NET Core
   - Identifies required changes
   - Suggests migration strategies

2. **Target Architecture:**
   - Backend: .NET 6/7/8 Web API
   - Frontend: React or Angular SPA
   - Database: AWS RDS (SQL Server or PostgreSQL)
   - Hosting: AWS Elastic Beanstalk or ECS

3. **Modernization Benefits:**
   - Cross-platform deployment (Linux containers)
   - Better performance
   - Modern development patterns
   - Cloud-native features
   - Improved scalability

## Testing

### Manual Testing

1. **Test Customer CRUD:**
   - Navigate to Customers
   - Create a new customer
   - Edit the customer
   - View details
   - Delete the customer

2. **Test Product CRUD:**
   - Navigate to Products
   - Create a new product (select a category)
   - Edit the product
   - View details
   - Delete the product

3. **Test Orders:**
   - Navigate to Orders
   - View order list
   - Click on an order to see details

### Database Testing

Use the provided SQL scripts:
- `beginner_test_commands.sql` - Basic queries
- `test_queries.sql` - Advanced queries

## Troubleshooting

### Common Issues

**1. Database Connection Error**
- Check SQL Server is running
- Verify connection string
- Ensure database exists
- Check Windows Authentication or SQL Auth credentials

**2. Missing DLL Errors**
- Restore NuGet packages
- Rebuild solution
- Check bin folder for required DLLs

**3. 404 Errors**
- Check routing configuration
- Verify controller and action names
- Check that views exist in correct folders

**4. Entity Framework Errors**
- Verify connection string
- Check that database schema matches models
- Ensure Entity Framework is installed

## License

This is a demonstration/PoC application for educational purposes.

## Support

For deployment help, see `DEPLOYMENT_GUIDE.md`

## Next Steps

1. Deploy to Windows Server 2019 with IIS
2. Test all functionality
3. Use AWS Porting Assistant to analyze for .NET Core migration
4. Plan modernization to .NET 6+ with React/Angular
5. Deploy modernized version to AWS

---

**Built with .NET Framework 4.8 | Ready for Modernization**
