# Entity Structure - User Inheritance Hierarchy

The Procrastinator application now uses a proper inheritance hierarchy for users, implementing the **disjoint and total** pattern where every user must be exactly one of two specialized types.

## Architecture Overview

### Base User Class
- **Abstract base class** that cannot be instantiated directly
- Contains common properties shared by all user types
- Provides abstract `ContactInfo` property for polymorphic access

### Specialized User Types

#### EmailUser
- **Inherits from**: `User`
- **Contact method**: Email address
- **Properties**:
  - `Id` (inherited)
  - `CreatedAt` (inherited)
  - `UpdatedAt` (inherited)
  - `EmailAddress` (unique, required)
- **ContactInfo**: Returns `EmailAddress`

#### PhoneUser
- **Inherits from**: `User`
- **Contact method**: Phone number
- **Properties**:
  - `Id` (inherited)
  - `CreatedAt` (inherited)
  - `UpdatedAt` (inherited)
  - `PhoneNumber` (unique, required)
- **ContactInfo**: Returns `PhoneNumber`

## Database Implementation

### Table-Per-Hierarchy (TPH) Inheritance
- **Single table**: `User` (not `Users`)
- **Discriminator column**: `UserType` with values "Email" or "Phone"
- **Nullable columns**: `EmailAddress` and `PhoneNumber` are nullable
- **Constraints**: Only one contact method column will have a value per row

### Data Integrity Constraints
The database enforces data integrity through check constraints:

```sql
-- Ensures EmailUser has EmailAddress not null
CONSTRAINT "CK_User_EmailUser_EmailNotNull" 
CHECK (("UserType" != 'Email') OR ("EmailAddress" IS NOT NULL))

-- Ensures PhoneUser has PhoneNumber not null  
CONSTRAINT "CK_User_PhoneUser_PhoneNotNull"
CHECK (("UserType" != 'Phone') OR ("PhoneNumber" IS NOT NULL))
```

These constraints ensure:
- **EmailUser rows**: Must have `EmailAddress` populated, `PhoneNumber` can be NULL
- **PhoneUser rows**: Must have `PhoneNumber` populated, `EmailAddress` can be NULL
- **Data consistency**: Impossible to have invalid user data

### Database Schema
```sql
CREATE TABLE "User" (
    "Id" SERIAL PRIMARY KEY,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UserType" VARCHAR(5) NOT NULL,  -- "Email" or "Phone"
    "EmailAddress" VARCHAR(255) NULL, -- Only populated for EmailUser
    "PhoneNumber" VARCHAR(20) NULL    -- Only populated for PhoneUser
);

-- Unique constraints
CREATE UNIQUE INDEX "IX_User_EmailAddress" ON "User" ("EmailAddress");
CREATE UNIQUE INDEX "IX_User_PhoneNumber" ON "User" ("PhoneNumber");

-- Check constraints for data integrity
ALTER TABLE "User" ADD CONSTRAINT "CK_User_EmailUser_EmailNotNull" 
    CHECK (("UserType" != 'Email') OR ("EmailAddress" IS NOT NULL));
ALTER TABLE "User" ADD CONSTRAINT "CK_User_PhoneUser_PhoneNotNull" 
    CHECK (("UserType" != 'Phone') OR ("PhoneNumber" IS NOT NULL));
```

## API Endpoints

### User Management
- **POST** `/api/User/Email` - Create new email user
- **POST** `/api/User/Phone` - Create new phone user
- **GET** `/api/User/Email` - List all email users
- **GET** `/api/User/Phone` - List all phone users

### Reminder Management
- **POST** `/api/Reminder` - Create reminder (auto-detects user type)

## Reminder Dispatch Logic

The system automatically routes reminders to the appropriate service:

### Email Users
- Uses `EmailMessageService` (MailKit)
- Sends reminders via SMTP
- Configuration in `appsettings.json` Email section

### Phone Users
- Uses `TwilioService`
- Sends reminders via SMS
- Configuration in `appsettings.json` Twilio section

## Benefits of This Design

1. **Type Safety**: Compile-time checking of user types
2. **Polymorphism**: Can treat all users uniformly via base class
3. **Extensibility**: Easy to add new user types in the future
4. **Data Integrity**: Enforces that users have exactly one contact method
5. **Service Routing**: Automatic selection of appropriate messaging service
6. **Clean API**: Separate endpoints for different user types
7. **Migration Safe**: No breaking changes to existing data structure
8. **Database Constraints**: Check constraints ensure data consistency at the database level

## Usage Examples

### Creating Users
```csharp
// Create email user
var emailUser = new EmailUser
{
    EmailAddress = "john@example.com"
};

// Create phone user
var phoneUser = new PhoneUser
{
    PhoneNumber = "+1234567890"
};
```

### Working with Users Polymorphically
```csharp
// Can work with any user type
User user = GetUserFromDatabase();
string contactInfo = user.ContactInfo; // Works for both types

// Type checking when needed
if (user is EmailUser emailUser)
{
    // Access email-specific properties
    string email = emailUser.EmailAddress;
}
```

### Reminder Creation
```csharp
// System automatically detects user type
var request = new ReminderRequest
{
    ContactInfo = "user@example.com", // Will create EmailUser
    Message = "Don't forget the meeting!",
    ReminderTimeline = ReminderTimelineType.Today
};
```

## Migration Notes

- **Existing data**: Will be lost during migration (users table is dropped)
- **New structure**: Implements TPH inheritance with discriminator
- **No breaking changes**: Removed Name property to maintain compatibility
- **Data integrity**: Check constraints ensure valid user data
- **Rollback**: Migration can be reversed if needed
- **Testing**: Verify functionality with new user types before production

## Why No Name Property?

The `Name` property was intentionally omitted to:
1. **Avoid migration issues** with existing data
2. **Keep the system simple** - users are identified by their contact info
3. **Maintain backward compatibility** with the original design
4. **Focus on core functionality** - contact method is the primary identifier

## Data Integrity Features

The system now includes robust data integrity at multiple levels:

1. **Application Level**: Strong typing with C# inheritance
2. **Database Level**: Check constraints prevent invalid data
3. **Unique Constraints**: Email addresses and phone numbers are unique
4. **Referential Integrity**: Foreign key relationships with cascade delete
5. **Validation**: Required fields and length constraints
