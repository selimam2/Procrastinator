# Procrastinator Backend

A .NET 8 Web API backend for managing procrastination reminders with smart scheduling. This is a **one-way API** where clients send reminders to the server, and the server handles them internally.

## Features

- **User Management**: Create users by phone number (auto-created when needed)
- **Smart Reminders**: Create reminders with intelligent scheduling based on timeline preferences
- **Database Integration**: PostgreSQL database with Entity Framework Core
- **One-Way Communication**: Clients send data, server processes internally
- **Automatic Message Dispatch**: Background service automatically sends reminders via configurable messaging providers
- **Extensible Architecture**: Easy to swap between SMS, email, push notifications, etc.
- **Smart Scheduling**: Calculates optimal reminder times based on timeline preferences
- **Simple Retry Logic**: Implicit interval-based retries through service cycle with configurable max attempts

## Prerequisites

- .NET 8 SDK
- PostgreSQL database
- Entity Framework Core tools
- Messaging service account (Twilio for SMS, SendGrid for email, etc.)

## Setup

### 1. Database Setup

1. Create a PostgreSQL database:
```sql
CREATE DATABASE procrastinator;
CREATE DATABASE procrastinator_dev;
```

2. Update the connection strings in `appsettings.json` and `appsettings.Development.json` with your database credentials.

3. Run the database creation script:
```bash
psql -d procrastinator -f Scripts/CreateTables.sql
```

4. (Optional) Insert sample data:
```bash
psql -d procrastinator -f Scripts/InsertSampleData.sql
```

### 2. Messaging Service Setup

#### Option A: Twilio SMS (Current Implementation)
1. Create a Twilio account at [twilio.com](https://www.twilio.com)
2. Get your Account SID and Auth Token from the Twilio Console
3. Get a Twilio phone number for sending SMS
4. Update the Twilio configuration in `appsettings.json`:
```json
"Twilio": {
  "AccountSid": "your_actual_account_sid",
  "AuthToken": "your_actual_auth_token",
  "FromPhoneNumber": "+1234567890"
}
```

#### Option B: Email Service
1. To use email instead, change the service registration in `Program.cs`:
```csharp
// Change this line:
builder.Services.AddScoped<IMessageService, TwilioService>();

// To this:
builder.Services.AddScoped<IMessageService, EmailMessageService>();
```

#### Option C: Custom Messaging Provider
1. Implement `IMessageService` interface
2. Register your service in `Program.cs`
3. No changes needed to the dispatch service!

### 3. App Configuration

Configure application behavior in `appsettings.json`:
```json
"AppSettings": {
  "MaxRetries": 3,
  "ServiceCheckIntervalMinutes": 1,
  "DefaultTimeZone": "UTC"
}
```

- **MaxRetries**: Maximum number of delivery attempts (default: 3)
- **ServiceCheckIntervalMinutes**: Minutes between service cycles (default: 1)
- **DefaultTimeZone**: Default timezone for the application (default: UTC)

### 4. Entity Framework Migrations

1. Install EF Core tools globally:
```bash
dotnet tool install --global dotnet-ef
```

2. Create initial migration:
```bash
dotnet ef migrations add InitialCreate
```

3. Update database:
```bash
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run
```

The API will be available at `https://localhost:7000` (or your configured port).

## API Endpoints

### Users
- `POST /api/User` - Create a new user

### Reminders
- `POST /api/Reminder` - Create a new reminder (auto-creates user if needed)

## How It Works

1. **Client sends reminder request** to `POST /api/Reminder`
2. **Server processes the request**:
   - Finds or creates user based on phone number
   - Calculates optimal reminder time using smart scheduling
   - Stores reminder in database with retry tracking
3. **Background service runs every configured interval**:
   - Checks for all incomplete reminders that haven't exhausted retries
   - Sends messages via configured messaging provider
   - Implements simple retry logic with implicit intervals
   - Marks reminders as completed after successful delivery
4. **Server handles everything internally** (no client retrieval needed)

## Background Services

### ReminderDispatchService
- **Runs every configured interval** (default: 1 minute) to check for due reminders
- **Automatically sends messages** via configured messaging provider when reminders are due
- **Implements simple retry logic** with configurable max attempts
- **Implicit intervals** - natural service cycle provides retry timing
- **Configurable timing** - adjust service check interval via AppSettings
- **Marks reminders as completed** after successful dispatch
- **Handles errors gracefully** with comprehensive logging
- **Provider-agnostic** - works with any messaging service

## App Configuration

### AppSettings Section
The system uses a centralized configuration approach with multiple settings:

```json
"AppSettings": {
  "MaxRetries": 3,
  "ServiceCheckIntervalMinutes": 1,
  "DefaultTimeZone": "UTC"
}
```

### Configuration Benefits
- **Centralized settings** - All app configuration in one place
- **Easy to adjust** - Change behavior without code changes
- **Environment-specific** - Different settings per environment
- **Type-safe access** - Strongly typed configuration values

## Retry Logic System

### Configuration
- **`MaxRetries`**: Maximum delivery attempts (configurable in AppSettings)

### Retry Behavior
- **Implicit intervals**: Service runs every configured interval, providing natural retry timing
- **Simple logic**: Just increment retry count on failure
- **No complex scheduling**: Failed reminders get retried on next service cycle
- **Service overload protection**: Natural delays prevent overwhelming messaging services

### Retry Fields
- **`RetryCount`**: Number of failed attempts (starts at 0)
- **`IsCompleted`**: Set to true after successful delivery

### How Retries Work
1. **Initial delivery**: When `ReminderDateTime` is reached
2. **On failure**: Increment `RetryCount`, save to database
3. **Next service cycle**: Service finds all incomplete reminders with `RetryCount < MaxRetries`
4. **Natural interval**: Configured service cycle provides retry timing
5. **After max retries**: Reminder marked as permanently failed

## Extensible Architecture

### IMessageService Interface
The system uses a generic `IMessageService` interface that can be implemented by different providers:

```csharp
public interface IMessageService
{
    Task<bool> SendMessageAsync(string recipient, string message);
}
```

### Available Implementations
- **`TwilioService`** - Sends SMS via Twilio (current default)
- **`EmailMessageService`** - Sends emails (example implementation)
- **Easy to add more** - Slack, Discord, Push Notifications, etc.

### Switching Providers
To change messaging providers, just update one line in `Program.cs`:

```csharp
// For SMS:
builder.Services.AddScoped<IMessageService, TwilioService>();

// For Email:
builder.Services.AddScoped<IMessageService, EmailMessageService>();

// For Custom Provider:
builder.Services.AddScoped<IMessageService, YourCustomService>();
```

## Data Models

### User
- `Id`: Primary key
- `PhoneNumber`: Unique phone number identifier
- `CreatedAt`: User creation timestamp
- `UpdatedAt`: Last update timestamp

### Reminder
- `Id`: Primary key
- `UserId`: Foreign key to User
- `Message`: Reminder message text
- `ReminderDateTime`: Calculated reminder time
- `ReminderTimeline`: Timeline preference (Today, Tomorrow, ThisWeek, etc.)
- `IsCompleted`: Completion status (set to true after successful delivery)
- `RetryCount`: Number of failed delivery attempts
- `CreatedAt`: Reminder creation timestamp
- `UpdatedAt`: Last update timestamp

## Smart Scheduling

The system automatically calculates optimal reminder times based on the selected timeline:

- **Today**: Random time between now and midnight
- **Tomorrow**: Random time tomorrow
- **ThisWeek**: Random time between now and end of week
- **NextWeek**: Random time next week
- **ThisMonth**: Random time between now and end of month
- **NextMonth**: Random time next month
- **LaterThisYear**: Random time between now and end of year
- **NextYear**: Random time next year

## Message Dispatch

- **Automatic**: No manual intervention required
- **Reliable**: Background service ensures delivery with retry logic
- **Tracked**: All message attempts and retries are logged and tracked
- **Error Handling**: Failed attempts trigger retries on next service cycle
- **Service Protection**: Natural service intervals prevent overwhelming messaging services
- **Provider-agnostic**: Works with any messaging service
- **Configurable**: Adjustable retry limits and service timing via AppSettings

## Swagger Documentation

Once running, visit `/swagger` to see the interactive API documentation.

## Development

- **One-way communication**: Clients only send data, never retrieve it
- **Automatic user creation**: Users are created automatically when creating reminders
- **Internal processing**: All reminder management happens server-side
- **Background processing**: Reminders are automatically dispatched via configurable messaging
- **Simple retry logic**: Implicit interval-based retries through service cycle
- **Centralized configuration**: All app settings in one place
- **Extensible architecture**: Easy to add new messaging providers
- **All timestamps are stored in UTC**
- **Database includes automatic triggers for updating `UpdatedAt` fields**
- **Proper indexing for performance on common queries**
