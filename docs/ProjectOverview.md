# 🏪 Inventory Management System

<div align="center">

[![Status](https://img.shields.io/badge/Status-Implemented-10B981?style=for-the-badge)](docs/ProjectOverview.md)
[![Phase](https://img.shields.io/badge/Phase-Development-3B82F6?style=for-the-badge)](docs/Documentation)
[![Team](https://img.shields.io/badge/Team-3_Members-8B5CF6?style=for-the-badge)](docs/Team)

[![Tech Stack](https://skillicons.dev/icons?i=dotnet,cs,bootstrap,visualstudio,git,github)](https://skillicons.dev)

</div>

## 📋 Executive Summary

<div align="center">

```mermaid
graph LR
    A[IMS] --> B[Product Management]
    A --> C[Stock Control]
    A --> D[Supplier Relations]
    A --> E[Reporting]
    
    style A fill:#3B82F6,stroke:#2563EB,color:white
    style B fill:#10B981,stroke:#059669,color:white
    style C fill:#8B5CF6,stroke:#7C3AED,color:white
    style D fill:#F59E0B,stroke:#D97706,color:white
    style E fill:#EC4899,stroke:#DB2777,color:white
```

</div>

### Project Overview
The Inventory Management System (IMS) is a comprehensive web-based solution built with ASP.NET Core 8.0 MVC, designed to streamline inventory management for small to medium-sized businesses. The system enables real-time tracking of products, suppliers, and stock levels through a secure, role-based interface.

### Business Value
- 📈 Reduce operational costs through optimized inventory levels
- 🎯 Prevent stockouts and overstock situations
- 💡 Data-driven decision making through detailed reporting
- ⚡ Improved efficiency in inventory operations
- 🤝 Better supplier relationship management

### Key Features Implemented
- 📦 **Complete Product Management**: CRUD operations, categorization, supplier associations, SKU management
- 🔄 **Stock Movement Tracking**: Detailed history of all inventory changes with audit trails
- 🔔 **Low Stock Notifications**: Automated alerts when inventory falls below thresholds
- 🤝 **Supplier Management**: Comprehensive supplier information and product relationships
- 📊 **Reports & Analytics**: Stock reports, transaction history, and business insights with export options
- 🔐 **Secure User Authentication**: Role-based access control with ASP.NET Core Identity
- 📱 **Responsive UI**: Mobile-friendly design using Bootstrap 5
- 📝 **Activity Logging**: Comprehensive audit trails of system activities
- 🧾 **Multiple Export Formats**: Support for PDF, CSV, and Excel exports
- 🔍 **Advanced Search & Filtering**: Real-time search and filtering capabilities
- 🎚️ **Soft Deletion**: Data preservation through soft delete functionality

## 📚 Technology Stack

| Component | Technology | Purpose |
|:----------|:-----------|:--------|
| **Framework** | ASP.NET Core 8.0 MVC | Web application framework |
| **Database** | SQL Server 2022 | Data storage and management |
| **ORM** | Entity Framework Core 8.0 | Data access and migrations |
| **Frontend** | Bootstrap 5, jQuery | Responsive UI and interactions |
| **Authentication** | ASP.NET Core Identity 8.0 | User management and security |
| **Validation** | FluentValidation | Input validation and error handling |
| **Object Mapping** | AutoMapper | Entity to DTO/ViewModel mapping |
| **Reporting** | QuestPDF, CsvHelper, ClosedXML | Export and reporting capabilities |
| **API Documentation** | Swashbuckle/Swagger | API documentation |
| **Logging** | Serilog | Application logging |
| **Dependency Injection** | Scrutor | Advanced dependency registration |

## 📦 Feature Visualizations

### Product Management Interface

```mermaid
classDiagram
    class ProductListView {
        Header: "Product Management"
        SearchBar: "Search products..."
        ActionButton: "+ Add Product"
        Pagination: "1-10 of 53 items"
        RefreshButton: "↻"
    }
    
    class ProductTable {
        Headers: ["SKU", "Product", "Category", "Stock", "Status", "Actions"]
        SortableColumns: true
        FilterableColumns: true
        ResponsiveDesign: true
    }
    
    class ProductRow {
        SKU: "PRD-001"
        Name: "Wireless Keyboard"
        Description: "Bluetooth RGB"
        Category: "Electronics"
        Stock: 42
        Status: "In Stock"
        Actions: ["View", "Edit", "Delete"]
    }
    
    class ProductRow2 {
        SKU: "PRD-002"
        Name: "USB-C Hub Adapter"
        Description: "Multiport"
        Category: "Electronics"
        Stock: 8
        Status: "Low Stock"
        Actions: ["View", "Edit", "Delete"]
    }
    
    class ProductRow3 {
        SKU: "PRD-003"
        Name: "HD Webcam"
        Description: "1080p with Microphone"
        Category: "Accessories"
        Stock: 0
        Status: "Out of Stock"
        Actions: ["View", "Edit", "Delete"]
    }
    
    class StatusIndicator {
        InStock: "🟢"
        LowStock: "🟠"
        OutOfStock: "🔴"
        BackOrdered: "🔵"
    }
    
    ProductListView *-- ProductTable
    ProductTable *-- ProductRow
    ProductTable *-- ProductRow2
    ProductTable *-- ProductRow3
    ProductRow *-- StatusIndicator
    ProductRow2 *-- StatusIndicator
    ProductRow3 *-- StatusIndicator
```

### Stock Management Workflow

```mermaid
flowchart LR
    A[Stock Dashboard] --> B{Action Type}
    B -->|Add Stock| C[Add Stock Form]
    B -->|Remove Stock| D[Remove Stock Form]
    B -->|Adjust Stock| E[Adjust Stock Form]
    B -->|View History| F[Stock History]
    
    C --> G[Enter Quantity]
    D --> H[Enter Quantity]
    E --> I[Enter Adjustment]
    
    G --> J[Add Notes]
    H --> K[Add Notes]
    I --> L[Add Notes]
    
    J --> M[Submit]
    K --> N[Submit]
    L --> O[Submit]
    
    M --> P[Update Inventory]
    N --> P
    O --> P
    
    P --> Q[Create Stock History Record]
    Q --> R[Check Stock Levels]
    R -->|Below Threshold| S[Generate Alert]
    R --> T[Update Dashboard]
    S --> T
    
    style A fill:#3B82F6,stroke:#2563EB,color:white
    style P fill:#10B981,stroke:#059669,color:white
    style Q fill:#8B5CF6,stroke:#7C3AED,color:white
    style S fill:#EF4444,stroke:#DC2626,color:white
    style T fill:#F59E0B,stroke:#D97706,color:white
```

### Supplier-Product Relationship Model

```mermaid
erDiagram
    SUPPLIER ||--o{ PRODUCT-SUPPLIER : provides
    PRODUCT ||--o{ PRODUCT-SUPPLIER : sourced_from
    CATEGORY ||--o{ PRODUCT : categorizes
    
    SUPPLIER {
        int id PK
        string name
        string contactPerson
        string email
        string phone
        string address
        boolean isActive
    }
    
    PRODUCT {
        int id PK
        string name
        string sku
        string description
        decimal price
        int categoryId FK
        int currentStock
        int minimumStockLevel
    }
    
    PRODUCT-SUPPLIER {
        int id PK
        int productId FK
        int supplierId FK
        decimal cost
        boolean isPrimarySupplier
        int leadTimeDays
        string notes
    }
    
    CATEGORY {
        int id PK
        string name
        string description
    }
```

### User Authentication & Management

```mermaid
flowchart TD
    A[User Access] --> B{Authentication}
    B -->|Login| C[Credential Validation]
    B -->|Register| D[New Account Creation]
    B -->|External Login| E[OAuth Provider]
    
    C -->|Success| F[User Dashboard]
    C -->|Failure| G[Login Error]
    D -->|Success| H[Email Confirmation]
    D -->|Failure| I[Registration Error]
    E --> F
    
    F --> J{Role-Based Access}
    
    J -->|Administrator| K[Full System Access]
    J -->|Manager| L[Products, Reports, Settings]
    J -->|Staff| M[Basic Inventory Operations]
    J -->|Viewer| N[Read-Only Access]
    
    subgraph "Identity Services"
    O[Identity Manager] --> P[Role Manager]
    O --> Q[Sign-In Manager]
    O --> R[Claims Principal]
    O --> S[Policy-Based Authorization]
    end
    
    C --- O
    D --- O
    J --- P
    
    style A fill:#3B82F6,stroke:#2563EB,color:white
    style J fill:#10B981,stroke:#059669,color:white
    style O fill:#8B5CF6,stroke:#7C3AED,color:white
    style P fill:#F59E0B,stroke:#D97706,color:white
    style S fill:#EC4899,stroke:#DB2777,color:white
```

### Reporting Architecture

```mermaid
flowchart TD
    A[Report Selection] --> B{Report Type}
    B -->|Stock Levels| C[Stock Level Report]
    B -->|Stock History| D[Movement Report]
    B -->|Value Analysis| E[Valuation Report]
    B -->|Supplier Analysis| F[Supplier Report]
    
    C --> G[Apply Filters]
    D --> G
    E --> G
    F --> G
    
    G --> H[Generate Report]
    
    H --> I{Export Format}
    I -->|CSV| J[CSV Export]
    I -->|PDF| K[PDF Generation]
    I -->|Excel| L[Excel Export]
    I -->|On-Screen| M[Display Report]
    
    J --> N[Download File]
    K --> N
    L --> N
    
    subgraph "Data Processing"
    O[Report Service] --> P[Repository Layer]
    P --> Q[Database]
    P --> R[Caching Layer]
    O --> S[Export Service]
    end
    
    H --- O
    
    style A fill:#3B82F6,stroke:#2563EB,color:white
    style H fill:#10B981,stroke:#059669,color:white
    style O fill:#8B5CF6,stroke:#7C3AED,color:white
    style Q fill:#F59E0B,stroke:#D97706,color:white
    style S fill:#EC4899,stroke:#DB2777,color:white
```

## 👥 System Roles

### User Access Levels

| Role | Permissions | Access Areas |
|:-----|:------------|:-------------|
| **Administrator** | Full system access | All areas + User Management |
| **Manager** | Read/write most data | Products, Reports, Settings |
| **Staff** | Limited write access | Basic inventory operations |
| **Viewer** | Read-only access | Dashboards and Read-only views |

## 📦 Documentation Structure

The project is documented across multiple files, each with a specific focus:

- **ProjectOverview.md** (this file): High-level overview, business value, feature visualizations
- **[SystemDesign.md](SystemDesign.md)**: Technical architecture, database design, security implementation
- **[ProjectStructure.md](ProjectStructure.md)**: Code organization, component details, design patterns

## 🚀 Future Enhancements

### Planned Features
- Advanced reporting and business intelligence
- Mobile application for inventory management
- Barcode/QR code integration for stock tracking
- Supplier portal for order management
- Multi-warehouse/location support
- Integration with accounting systems
- API endpoints for third-party integration

## 🎓 Program Details

- **Initiative**: Digital Egypt Pioneers Initiative (DEPI)
- **Track**: Full Stack .NET Developer
- **Cohort**: 2024-2025
- **Mentor**: Eng. Amira Hashim
- **Project Type**: Graduation Project

---

<div align="center">

*A Graduation Project for the Digital Egypt Pioneers Initiative (DEPI)*

</div>