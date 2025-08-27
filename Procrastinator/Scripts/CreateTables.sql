-- Create Users table
CREATE TABLE Users (
    Id SERIAL PRIMARY KEY,
    PhoneNumber VARCHAR(20) UNIQUE NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create Reminders table
CREATE TABLE Reminders (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER NOT NULL,
    Message TEXT NOT NULL,
    ReminderDateTime TIMESTAMP WITH TIME ZONE NOT NULL,
    ReminderTimeline VARCHAR(20) NOT NULL,
    IsCompleted BOOLEAN DEFAULT FALSE,
    RetryCount INTEGER DEFAULT 0,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Create indexes for better performance
CREATE INDEX IX_Reminders_UserId ON Reminders(UserId);
CREATE INDEX IX_Reminders_ReminderDateTime ON Reminders(ReminderDateTime);
CREATE INDEX IX_Reminders_IsCompleted ON Reminders(IsCompleted);
CREATE INDEX IX_Reminders_RetryCount ON Reminders(RetryCount);

-- Create function to update UpdatedAt timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.UpdatedAt = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Create triggers to automatically update UpdatedAt
CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON Users
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_reminders_updated_at BEFORE UPDATE ON Reminders
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
