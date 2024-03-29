CREATE TABLE IF NOT EXISTS users 
(
    UserId INTEGER PRIMARY KEY AUTO_INCREMENT,
    FirstName VARCHAR(80),
    LastName VARCHAR(80),
    Password VARCHAR(80),
    Email VARCHAR(80)
);

CREATE TABLE IF NOT EXISTS addresses
(
    AddressId INTEGER PRIMARY KEY AUTO_INCREMENT,
    UserId INT,
    AddressString VARCHAR(100),
    FOREIGN KEY (UserId) REFERENCES users(UserId)
);

CREATE TABLE IF NOT EXISTS products
(
    ProductId INTEGER PRIMARY KEY AUTO_INCREMENT,
    NameProduct VARCHAR(100),
    TypeProduct VARCHAR(100),
    DescriptionProduct VARCHAR(500),
    Price REAL,
    StatusProduct INT
);

CREATE TABLE IF NOT EXISTS carts
(
    CartId INTEGER PRIMARY KEY AUTO_INCREMENT,
    UserId INT,
    Price REAL,
    StatusProduct INT,
    FOREIGN KEY (UserId) REFERENCES users(UserId)
);

CREATE TABLE IF NOT EXISTS commands
(
    CommandId INTEGER PRIMARY KEY AUTO_INCREMENT,
    UserId INT,
    Price REAL,
    StatusProduct INT,
    FOREIGN KEY (UserId) REFERENCES users(UserId)
);

CREATE TABLE IF NOT EXISTS invoices
(
    InvoiceId INTEGER PRIMARY KEY AUTO_INCREMENT,
    UserId INT,
    Price REAL,
    StatusProduct INT,
    FOREIGN KEY (UserId) REFERENCES users(UserId)
);

CREATE TABLE IF NOT EXISTS carts_products
(
    CartProductId INTEGER PRIMARY KEY AUTO_INCREMENT,
    CartId INTEGER,
    ProductId INTEGER,
    FOREIGN KEY (CartId) REFERENCES carts(CartId),
    FOREIGN KEY (ProductId) REFERENCES products(ProductId)
);

CREATE TABLE IF NOT EXISTS commands_products
(
    CommandProductId INTEGER PRIMARY KEY AUTO_INCREMENT,
    CommandId INTEGER,
    ProductId INTEGER,
    FOREIGN KEY (CommandId) REFERENCES commands(CommandId),
    FOREIGN KEY (ProductId) REFERENCES products(ProductId)
);

CREATE TABLE IF NOT EXISTS invoices_products
(
    InvoiceProductId INTEGER PRIMARY KEY AUTO_INCREMENT,
    InvoiceId INTEGER,
    ProductId INTEGER,
    FOREIGN KEY (InvoiceId) REFERENCES invoices(InvoiceId),
    FOREIGN KEY (ProductId) REFERENCES products(ProductId)
);
