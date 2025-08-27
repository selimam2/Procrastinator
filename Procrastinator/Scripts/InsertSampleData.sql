-- Insert sample users
INSERT INTO Users (PhoneNumber) VALUES 
('+1234567890'),
('+0987654321'),
('+5555555555');

-- Insert sample reminders
INSERT INTO Reminders (UserId, Message, ReminderDateTime, ReminderTimeline) VALUES 
(1, 'Call mom about dinner plans', CURRENT_TIMESTAMP + INTERVAL '2 hours', 'Today'),
(1, 'Submit quarterly report', CURRENT_TIMESTAMP + INTERVAL '1 day', 'Tomorrow'),
(2, 'Buy groceries for the week', CURRENT_TIMESTAMP + INTERVAL '3 days', 'ThisWeek'),
(3, 'Schedule dentist appointment', CURRENT_TIMESTAMP + INTERVAL '1 week', 'NextWeek');
