# Rules For AI

## Basic

> **Project:** OpenKnowledge

> **Frameworks:** **C# Modern (.NET 8.0+) MVVM Avalonia 11.3.0+ (MVVM CommunityToolKit)**

---

## Code Style Guide 

Here's a set of rules for writing clean, maintainable C# code:

### Essential Rules

1. **File Scoped Namespaces**
   ```csharp
   // CORRECT: Use file scoped namespaces
   namespace Company.Product.Module;
   
   // INCORRECT: Don't use block-style namespaces
   namespace Company.Product.Module 
   {
       // Code here
   }
   ```

2. **Language for User-Facing Messages**
   ```csharp
   // CORRECT: English messages
   throw new ArgumentException("Value cannot be null or empty.");
   
   // INCORRECT: Non-English messages can break console encoding
   throw new ArgumentException("値がnullまたは空であってはなりません。");
   ```

3. **Using Declarations**
   ```csharp
   // CORRECT: Modern using declaration (C# 8.0+)
   using var reader = new StreamReader(path);
   var content = reader.ReadToEnd();
   
   // INCORRECT: Older using statement with blocks
   using (var reader = new StreamReader(path))
   {
       var content = reader.ReadToEnd();
   }
   ```

### Naming Conventions

4. **Pascal Case for Types and Members**
   ```csharp
   public class CustomerService
   {
       public void ProcessOrder(Order order) { }
   }
   ```

5. **Camel Case for Local Variables and Parameters**
   ```csharp
   public void ProcessOrder(Order order)
   {
       var customerName = order.Customer.Name;
   }
   ```

6. **Interface Names Start with I**
   ```csharp
   public interface IOrderProcessor
   {
       Task ProcessAsync(Order order);
   }
   ```

## Code Organization

7. **One Type Per File**
   ```csharp
   // CustomerService.cs
   namespace Company.Services;
   
   public class CustomerService
   {
       // Implementation
   }
   ```

8. **Organize Usings**
   ```csharp
   // System namespaces first
   using System;
   using System.Collections.Generic;
   
   // Then third-party
   using Microsoft.Extensions.Logging;
   
   // Then your own namespaces
   using Company.Core.Models;
   ```

### Modern C# Features

9. **Use Expression-Bodied Members When Simple**
   ```csharp
   // CORRECT: Simple method with expression body
   public string GetFullName() => $"{FirstName} {LastName}";
   
   // INCORRECT: Unnecessary block for simple expression
   public string GetFullName()
   {
       return $"{FirstName} {LastName}";
   }
   ```

10. **Use Pattern Matching**
    ```csharp
    // CORRECT: Modern pattern matching
    if (obj is Customer customer && customer.IsActive)
    {
        // Use customer directly
    }
    
    // INCORRECT: Type checking and casting separately
    if (obj is Customer && ((Customer)obj).IsActive)
    {
        var customer = (Customer)obj;
        // Use customer
    }
    ```

11. **Use Null Checks with ??/?./?[]**
    ```csharp
    // CORRECT: Null conditional operator
    var name = customer?.Name ?? "Unknown";
    
    // CORRECT Too: Verbose null checking (Keep Clean)
    var name = customer == null ? "Unknown" : customer.Name;
    ```

### Error Handling

12. **Prefer Exceptions for Exceptional Conditions Only**
    ```csharp
    // CORRECT: Exception for truly exceptional condition
    if (string.IsNullOrEmpty(filePath))
        throw new ArgumentException("File path cannot be empty", nameof(filePath));
    
    // INCORRECT: Using exceptions for normal control flow
    try {
        var data = GetData();
        ProcessData(data);
    } catch (DataNotFoundException) {
        // Don't use exceptions for expected conditions
    }
    ```

13. **Include Parameter Names in ArgumentExceptions**
    ```csharp
    // CORRECT: Include parameter name
    throw new ArgumentNullException(nameof(customer), "Customer cannot be null");
    
    // INCORRECT: Missing parameter name
    throw new ArgumentNullException("Customer cannot be null");
    ```

### Asynchronous Programming

14. **Use Async/Await Consistently**
    ```csharp
    // CORRECT: Proper async/await pattern
    public async Task<Result> ProcessOrderAsync(Order order)
    {
        var data = await _repository.GetAsync(order.Id);
        return await _processor.RunAsync(data);
    }
    
    // INCORRECT: Missing await
    public async Task<Result> ProcessOrderAsync(Order order)
    {
        var data = _repository.GetAsync(order.Id).Result; // Blocks thread
        return _processor.RunAsync(data).Result;          // Blocks thread
    }
    ```

15. **Use ConfigureAwait(false) in Libraries**
    ```csharp
    // In libraries:
    await httpClient.GetAsync(url).ConfigureAwait(false);
    
    // In application code, ConfigureAwait(false) is usually unnecessary
    await httpClient.GetAsync(url);
    ```

### Code Quality

16. **Prefer Readability Over Cleverness**
    ```csharp
    // CORRECT: Clear and explicit
    var activeCustomers = customers.Where(c => c.IsActive).ToList();
    
    // INCORRECT: Too clever, harder to understand
    var actives = customers.Where(c => c switch { { IsActive: true } => true, _ => false });
    ```

17. **Comment Why, Not What**
    ```csharp
    // CORRECT: Explains why
    // Skip validation for system accounts to allow special characters
    if (!account.IsSystemAccount)
    {
        ValidateUsername(account.Username);
    }
    
    // INCORRECT: Explains what (the code already shows this)
    // Check if not system account
    if (!account.IsSystemAccount)
    {
        // Validate the username
        ValidateUsername(account.Username);
    }
    ```

---

## Project Code Quality Rules

1. Use `bool throwExceptions = false` to control exception behavior
   ```csharp
   public int? DoSomething(bool throwExceptions = false)
   {
       try
       {
           // Code here
       }
       catch (Exception ex) when (!throwExceptions)
       {
           // Return null on exception if throwExceptions is false
           return null;
       }
   }
   ```

2. Use I18n internationalization support
   ```csharp
   // XAML
   Title="{loc:Translate Key=main.title}"
 
   // C#
   var localizationService = App.GetService<LocalizationService>();
   var data = localizationService["main.title"]; // Use key to get localized string
   ```

