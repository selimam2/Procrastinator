# Email Service Configuration

The Procrastinator application now includes a fully functional email service using MailKit. This service uses the `AppConfiguration` pattern to read email settings from configuration files.

## Configuration

Update your `appsettings.json` or `appsettings.Development.json` file with your email provider settings:

### Gmail Configuration
```json
"Email": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "Username": "your-email@gmail.com",
  "Password": "your-app-password",
  "FromEmail": "your-email@gmail.com",
  "FromName": "Procrastinator",
  "EnableSsl": true,
  "UseDefaultCredentials": false,
  "TimeoutSeconds": 30
}
```

**Important**: For Gmail, you need to use an "App Password" instead of your regular password. Enable 2-factor authentication and generate an app password.

### Outlook/Hotmail Configuration
```json
"Email": {
  "SmtpServer": "smtp-mail.outlook.com",
  "SmtpPort": 587,
  "Username": "your-email@outlook.com",
  "Password": "your-password",
  "FromEmail": "your-email@outlook.com",
  "FromName": "Procrastinator",
  "EnableSsl": true,
  "UseDefaultCredentials": false,
  "TimeoutSeconds": 30
}
```

### Yahoo Configuration
```json
"Email": {
  "SmtpServer": "smtp.mail.yahoo.com",
  "SmtpPort": 587,
  "Username": "your-email@yahoo.com",
  "Password": "your-app-password",
  "FromEmail": "your-email@yahoo.com",
  "FromName": "Procrastinator",
  "EnableSsl": true,
  "UseDefaultCredentials": false,
  "TimeoutSeconds": 30
}
```

### Custom SMTP Server
```json
"Email": {
  "SmtpServer": "your-smtp-server.com",
  "SmtpPort": 587,
  "Username": "your-username",
  "Password": "your-password",
  "FromEmail": "noreply@yourdomain.com",
  "FromName": "Procrastinator",
  "EnableSsl": true,
  "UseDefaultCredentials": false,
  "TimeoutSeconds": 30
}
```

## How It Works

The email service uses the `AppConfiguration` class to read email settings. This class is automatically populated from your configuration files and provides strongly-typed access to all email settings:

- `EmailSmtpServer` - SMTP server address
- `EmailSmtpPort` - SMTP server port
- `EmailUsername` - Authentication username
- `EmailPassword` - Authentication password
- `EmailFromEmail` - Sender email address
- `EmailFromName` - Sender display name
- `EmailEnableSsl` - Whether to enable SSL/TLS
- `EmailUseDefaultCredentials` - Whether to use default credentials
- `EmailTimeoutSeconds` - Connection timeout in seconds

## Security Notes

1. **Never commit real credentials to source control**
2. **Use environment variables or user secrets for production**
3. **Enable SSL/TLS for secure communication**
4. **Use app passwords for services that support them**

## Environment Variables

You can also configure email settings using environment variables:

```bash
Email__SmtpServer=smtp.gmail.com
Email__SmtpPort=587
Email__Username=your-email@gmail.com
Email__Password=your-app-password
Email__FromEmail=your-email@gmail.com
Email__FromName=Procrastinator
```

## Testing

To test the email service, you can use the reminder functionality in the application. The service will log success/failure messages to help with debugging.

## Troubleshooting

- **Authentication failed**: Check username/password and ensure app passwords are used where required
- **Connection timeout**: Verify SMTP server and port settings
- **SSL errors**: Ensure EnableSsl is set to true for most modern email providers
- **Configuration not found**: Verify that the Email section exists in your appsettings.json file
