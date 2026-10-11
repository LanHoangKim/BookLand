/* =============================================================
   BOOKLAND - CƠ SỞ DỮ LIỆU WEBSITE NHÀ SÁCH (tham khảo fahasa.com)
   Chạy file này trước, sau đó chạy 02_SeedData.sql
   Yêu cầu: SQL Server 2016 trở lên
   ============================================================= */
USE master;
GO

IF DB_ID(N'BookLandDB') IS NOT NULL
BEGIN
    ALTER DATABASE BookLandDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BookLandDB;
END
GO

-- Vietnamese_CI_AI: tìm kiếm không phân biệt hoa/thường và dấu (vd: "nha gia kim" vẫn ra "Nhà Giả Kim")
CREATE DATABASE BookLandDB COLLATE Vietnamese_CI_AI;
GO

USE BookLandDB;
GO

/* ---------------- CÁC BẢNG DANH MỤC (bảng phụ) ---------------- */

-- Danh mục sản phẩm, 2 cấp (ParentID = NULL là danh mục cha)
CREATE TABLE Category (
    CategoryID      INT IDENTITY(1,1) NOT NULL,
    CategoryName    NVARCHAR(100)     NOT NULL,
    ParentID        INT               NULL,
    ImageUrl        NVARCHAR(255)     NULL,
    Description     NVARCHAR(500)     NULL,
    SortOrder       INT               NOT NULL CONSTRAINT DF_Category_SortOrder DEFAULT (0),
    IsActive        BIT               NOT NULL CONSTRAINT DF_Category_IsActive DEFAULT (1),
    CONSTRAINT PK_Category PRIMARY KEY (CategoryID),
    CONSTRAINT FK_Category_Parent FOREIGN KEY (ParentID) REFERENCES Category (CategoryID)
);
GO

-- Tác giả
CREATE TABLE Author (
    AuthorID        INT IDENTITY(1,1) NOT NULL,
    AuthorName      NVARCHAR(150)     NOT NULL,
    Nationality     NVARCHAR(50)      NULL,
    BirthYear       INT               NULL,
    Biography       NVARCHAR(1000)    NULL,
    CONSTRAINT PK_Author PRIMARY KEY (AuthorID),
    CONSTRAINT CK_Author_BirthYear CHECK (BirthYear IS NULL OR BirthYear BETWEEN 1000 AND 2100)
);
GO

-- Nhà xuất bản
CREATE TABLE Publisher (
    PublisherID     INT IDENTITY(1,1) NOT NULL,
    PublisherName   NVARCHAR(150)     NOT NULL,
    Address         NVARCHAR(255)     NULL,
    Phone           VARCHAR(20)       NULL,
    Email           VARCHAR(100)      NULL,
    Website         VARCHAR(150)      NULL,
    CONSTRAINT PK_Publisher PRIMARY KEY (PublisherID)
);
GO

-- Nhà cung cấp (công ty phát hành)
CREATE TABLE Supplier (
    SupplierID      INT IDENTITY(1,1) NOT NULL,
    SupplierName    NVARCHAR(150)     NOT NULL,
    Address         NVARCHAR(255)     NULL,
    Phone           VARCHAR(20)       NULL,
    Email           VARCHAR(100)      NULL,
    Website         VARCHAR(150)      NULL,
    CONSTRAINT PK_Supplier PRIMARY KEY (SupplierID)
);
GO

-- Hình thức bìa (Bìa mềm, Bìa cứng, ...)
CREATE TABLE CoverType (
    CoverTypeID     INT IDENTITY(1,1) NOT NULL,
    CoverTypeName   NVARCHAR(50)      NOT NULL,
    CONSTRAINT PK_CoverType PRIMARY KEY (CoverTypeID),
    CONSTRAINT UQ_CoverType_Name UNIQUE (CoverTypeName)
);
GO

/* ---------------- HÀNG HÓA ---------------- */

-- Sản phẩm: thông tin hiển thị chính
CREATE TABLE Product (
    ProductID       INT IDENTITY(1,1) NOT NULL,
    ProductName     NVARCHAR(255)     NOT NULL,
    CategoryID      INT               NOT NULL,
    AuthorID        INT               NULL,
    PublisherID     INT               NULL,
    SupplierID      INT               NULL,
    Price           DECIMAL(18,0)     NOT NULL,   -- giá bán
    OriginalPrice   DECIMAL(18,0)     NULL,       -- giá bìa (hiển thị gạch ngang, tính % giảm)
    ImageUrl        NVARCHAR(255)     NULL,       -- ảnh đại diện
    Description     NVARCHAR(MAX)     NULL,
    IsFeatured      BIT               NOT NULL CONSTRAINT DF_Product_IsFeatured DEFAULT (0),   -- sản phẩm nổi bật
    IsFlashSale     BIT               NOT NULL CONSTRAINT DF_Product_IsFlashSale DEFAULT (0),  -- tham gia Flash Sale
    IsActive        BIT               NOT NULL CONSTRAINT DF_Product_IsActive DEFAULT (1),
    CreatedAt       DATETIME          NOT NULL CONSTRAINT DF_Product_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT PK_Product PRIMARY KEY (ProductID),
    CONSTRAINT FK_Product_Category  FOREIGN KEY (CategoryID)  REFERENCES Category (CategoryID),
    CONSTRAINT FK_Product_Author    FOREIGN KEY (AuthorID)    REFERENCES Author (AuthorID),
    CONSTRAINT FK_Product_Publisher FOREIGN KEY (PublisherID) REFERENCES Publisher (PublisherID),
    CONSTRAINT FK_Product_Supplier  FOREIGN KEY (SupplierID)  REFERENCES Supplier (SupplierID),
    CONSTRAINT CK_Product_Price CHECK (Price >= 0),
    CONSTRAINT CK_Product_OriginalPrice CHECK (OriginalPrice IS NULL OR OriginalPrice >= Price)
);
GO

-- Chi tiết sản phẩm: thông số kỹ thuật + tồn kho (mỗi sản phẩm có 1 dòng)
CREATE TABLE ProductDetail (
    ProductDetailID INT IDENTITY(1,1) NOT NULL,
    ProductID       INT               NOT NULL,
    SKU             VARCHAR(20)       NULL,       -- mã hàng
    Translator      NVARCHAR(150)     NULL,       -- người dịch
    PublishYear     INT               NULL,
    Language        NVARCHAR(50)      NULL,
    Weight          INT               NULL,       -- trọng lượng (gram)
    Dimensions      NVARCHAR(50)      NULL,       -- kích thước bao bì
    PageCount       INT               NULL,
    CoverTypeID     INT               NULL,
    Quantity        INT               NOT NULL CONSTRAINT DF_ProductDetail_Quantity DEFAULT (0),      -- số lượng tồn
    SoldQuantity    INT               NOT NULL CONSTRAINT DF_ProductDetail_SoldQuantity DEFAULT (0),  -- số lượng đã bán
    CONSTRAINT PK_ProductDetail PRIMARY KEY (ProductDetailID),
    CONSTRAINT UQ_ProductDetail_Product UNIQUE (ProductID),
    CONSTRAINT FK_ProductDetail_Product   FOREIGN KEY (ProductID)   REFERENCES Product (ProductID),
    CONSTRAINT FK_ProductDetail_CoverType FOREIGN KEY (CoverTypeID) REFERENCES CoverType (CoverTypeID),
    CONSTRAINT CK_ProductDetail_Quantity CHECK (Quantity >= 0),
    CONSTRAINT CK_ProductDetail_SoldQuantity CHECK (SoldQuantity >= 0),
    CONSTRAINT CK_ProductDetail_PageCount CHECK (PageCount IS NULL OR PageCount > 0)
);
GO

-- Ảnh phụ của sản phẩm (thư viện ảnh ở trang chi tiết)
CREATE TABLE ProductImage (
    ProductImageID  INT IDENTITY(1,1) NOT NULL,
    ProductID       INT               NOT NULL,
    ImageUrl        NVARCHAR(255)     NOT NULL,
    SortOrder       INT               NOT NULL CONSTRAINT DF_ProductImage_SortOrder DEFAULT (0),
    CONSTRAINT PK_ProductImage PRIMARY KEY (ProductImageID),
    CONSTRAINT FK_ProductImage_Product FOREIGN KEY (ProductID) REFERENCES Product (ProductID)
);
GO

/* ---------------- NGƯỜI DÙNG ---------------- */

-- Khách hàng (đăng ký trên website)
CREATE TABLE Customer (
    CustomerID      INT IDENTITY(1,1) NOT NULL,
    FullName        NVARCHAR(100)     NOT NULL,
    Email           VARCHAR(100)      NOT NULL,
    Phone           VARCHAR(15)       NOT NULL,
    PasswordHash    VARCHAR(255)      NOT NULL,
    Gender          NVARCHAR(10)      NULL,
    BirthDate       DATE              NULL,
    Address         NVARCHAR(255)     NULL,
    CreatedAt       DATETIME          NOT NULL CONSTRAINT DF_Customer_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT PK_Customer PRIMARY KEY (CustomerID),
    CONSTRAINT UQ_Customer_Email UNIQUE (Email),
    CONSTRAINT UQ_Customer_Phone UNIQUE (Phone)
);
GO

-- Người quản trị (đăng nhập trang Admin)
CREATE TABLE AdminUser (
    AdminID         INT IDENTITY(1,1) NOT NULL,
    Username        VARCHAR(50)       NOT NULL,
    FullName        NVARCHAR(100)     NOT NULL,
    Email           VARCHAR(100)      NULL,
    Role            NVARCHAR(20)      NOT NULL CONSTRAINT DF_AdminUser_Role DEFAULT (N'Quản trị viên'),
    PasswordHash    VARCHAR(255)      NOT NULL,
    CreatedAt       DATETIME          NOT NULL CONSTRAINT DF_AdminUser_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT PK_AdminUser PRIMARY KEY (AdminID),
    CONSTRAINT UQ_AdminUser_Username UNIQUE (Username)
);
GO

/* ---------------- ĐƠN HÀNG ---------------- */

CREATE TABLE Orders (
    OrderID         INT IDENTITY(1,1) NOT NULL,
    CustomerID      INT               NOT NULL,
    OrderDate       DATETIME          NOT NULL CONSTRAINT DF_Orders_OrderDate DEFAULT (GETDATE()),
    ReceiverName    NVARCHAR(100)     NOT NULL,
    ReceiverPhone   VARCHAR(15)       NOT NULL,
    ShippingAddress NVARCHAR(255)     NOT NULL,
    Note            NVARCHAR(500)     NULL,
    PaymentMethod   NVARCHAR(50)      NOT NULL CONSTRAINT DF_Orders_PaymentMethod DEFAULT (N'Thanh toán khi nhận hàng'),
    ShippingFee     DECIMAL(18,0)     NOT NULL CONSTRAINT DF_Orders_ShippingFee DEFAULT (0),
    TotalAmount     DECIMAL(18,0)     NOT NULL,   -- tổng tiền (đã gồm phí vận chuyển)
    Status          NVARCHAR(30)      NOT NULL CONSTRAINT DF_Orders_Status DEFAULT (N'Chờ xác nhận'),
    CONSTRAINT PK_Orders PRIMARY KEY (OrderID),
    CONSTRAINT FK_Orders_Customer FOREIGN KEY (CustomerID) REFERENCES Customer (CustomerID),
    CONSTRAINT CK_Orders_TotalAmount CHECK (TotalAmount >= 0)
);
GO

CREATE TABLE OrderDetail (
    OrderDetailID   INT IDENTITY(1,1) NOT NULL,
    OrderID         INT               NOT NULL,
    ProductID       INT               NOT NULL,
    Quantity        INT               NOT NULL,
    UnitPrice       DECIMAL(18,0)     NOT NULL,   -- đơn giá tại thời điểm mua
    Amount          DECIMAL(18,0)     NOT NULL,   -- thành tiền = Quantity * UnitPrice
    CONSTRAINT PK_OrderDetail PRIMARY KEY (OrderDetailID),
    CONSTRAINT FK_OrderDetail_Orders  FOREIGN KEY (OrderID)   REFERENCES Orders (OrderID),
    CONSTRAINT FK_OrderDetail_Product FOREIGN KEY (ProductID) REFERENCES Product (ProductID),
    CONSTRAINT CK_OrderDetail_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_OrderDetail_UnitPrice CHECK (UnitPrice >= 0)
);
GO

/* ---------------- CHỈ MỤC ---------------- */
CREATE INDEX IX_Category_ParentID     ON Category (ParentID);
CREATE INDEX IX_Product_CategoryID    ON Product (CategoryID);
CREATE INDEX IX_Product_AuthorID      ON Product (AuthorID);
CREATE INDEX IX_Product_PublisherID   ON Product (PublisherID);
CREATE INDEX IX_Product_SupplierID    ON Product (SupplierID);
CREATE INDEX IX_ProductImage_ProductID ON ProductImage (ProductID);
CREATE INDEX IX_Orders_CustomerID     ON Orders (CustomerID);
CREATE INDEX IX_OrderDetail_OrderID   ON OrderDetail (OrderID);
CREATE INDEX IX_OrderDetail_ProductID ON OrderDetail (ProductID);
GO

PRINT N'Đã tạo xong cơ sở dữ liệu BookLandDB. Tiếp theo hãy chạy 02_SeedData.sql';
GO
