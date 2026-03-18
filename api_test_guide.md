# POS.Api - Complete Testing Guide

This guide provides exactly what you need to test the entire POS API flow using **Swagger UI** (available at `http://localhost:5277/swagger` when you run the app). 

---

## Step 1: Authentication & Setup

Since we seeded [Branches](file:///c:/Users/Dinushka%20Sandeepa/OneDrive/Desktop/POS%20system/POS.Api/Controllers/BranchesController.cs#9-76), we first need to register an Admin user for `Branch 1` and get our JWT Token.

### 1. Register an Admin User
**Endpoint:** `POST /api/Auth/register`
**Payload:**
```json
{
  "fullName": "Admin User",
  "email": "admin@pos.com",
  "password": "Password123!",
  "role": "Admin",
  "branchId": 1
}
```
*Expected: 200 OK with `token` and `refreshToken`.*

### 2. Login & Authorize Swagger
**Endpoint:** `POST /api/Auth/login`
**Payload:**
```json
{
  "email": "admin@pos.com",
  "password": "Password123!"
}
```
*Copy the `token` string from the response. Scroll to the top of Swagger, click the green **Authorize** button, type `Bearer YOUR_TOKEN_HERE`, and click Authorize. Now all your secured requests will work!*

---

## Step 2: Manage Products

We already seeded 3 Categories (1=Electronics, 2=Beverages, 3=Snacks). Let's create a product.

### 1. Create a Product
**Endpoint:** `POST /api/Products`
**Payload:**
```json
{
  "name": "Coca Cola 500ml",
  "barcode": "8594002",
  "price": 150.00,
  "costPrice": 100.00,
  "categoryId": 2
}
```
*Expected: 201 Created. Note the `id` of the created product (it should be 1).*

### 2. Verify Product List
**Endpoint:** `GET /api/Products`
*Expected: A list containing your new Coca Cola product.*

---

## Step 3: Stock Management

Let's add some stock for the product so we can sell it.

### 1. Add Stock Adjustment
**Endpoint:** `PATCH /api/Stock/{productId}/adjust`
**Parameters:** `productId = 1`
**Payload:**
```json
{
  "branchId": 1,
  "adjustment": 50,
  "reason": "Purchase"
}
```
*Expected: 204 No Content. You just added 50 units to Branch 1.*

### 2. View Stock Audit Log
**Endpoint:** `GET /api/Stock/audit/{productId}`
**Parameters:** `productId = 1`
*Expected: Shows the history of who added the 50 items and why.*

---

## Step 4: Customers

Let's create a loyal customer to attach to our upcoming order.

### 1. Create Customer
**Endpoint:** `POST /api/Customers`
**Payload:**
```json
{
  "fullName": "John Doe",
  "phone": "0771234567",
  "email": "john@example.com"
}
```
*Expected: 201 Created. Note the `id` (should be 1).*

---

## Step 5: Sales & Orders

Time to make a sale! We will sell 2 Coca Colas to John Doe.

### 1. Create a New POS Order
**Endpoint:** `POST /api/Orders`
**Payload:**
```json
{
  "customerId": 1,
  "branchId": 1,
  "paymentMode": 0,  
  "cashAmount": 300,
  "cardAmount": 0,
  "items": [
    {
      "productId": 1,
      "quantity": 2,
      "discountAmount": 0
    }
  ]
}
```
*(Note: `paymentMode: 0` is Cash. The total is 300 because Coca Cola is 150 each).*
*Expected: 201 Created. The stock of Coca Cola is now automatically reduced by 2, and John Doe earned loyalty points.*

### 2. Verify Stock Deduction
**Endpoint:** `GET /api/Stock/low`
**Parameters:** `branchId = 1`
*Run this just to see if stock reduced from 50 to 48.*

---

## Step 6: Reports & Receipts

Finally, let's view the sales report and generate the PDF receipt.

### 1. View Daily Sales Summary
**Endpoint:** `GET /api/Reports/summary`
**Parameters:** 
- `branchId = 1`
- `date = 2024-03-18` (Use today's date)
*Expected: Retrieves total revenue (300) and order count (1).*

### 2. Download PDF Receipt for the Order
**Endpoint:** `GET /api/Reports/receipt/{orderId}`
**Parameters:** `orderId = 1`
*Expected: 200 OK. Swagger will let you "Download file" which will be a beautifully formatted Thermal Receipt PDF containing the Coca Cola sale!*
