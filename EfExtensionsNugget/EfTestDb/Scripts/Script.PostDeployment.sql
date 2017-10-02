
INSERT INTO [dbo].[Courses]
           ([CourseName])
     VALUES ('Chemistry'),('Algebra'),('Physics'),('Astronomy')
GO

INSERT INTO [dbo].[Students]
           ([FirstName]
           ,[LastName])
     VALUES
           ('Vasiliy','Petrov'),
		 ('Petr','Stepanov'),
		 ('Aleksandr','Matrosov'),
		 ('Stepan','Pechkin'),
		 ('Pavlik','Morozov'),
		 ('Fedor','Rurik'),
		 ('Ivan','Grozniy')
GO

INSERT INTO [dbo].[Registrations]
SELECT TOP 20 CourseId,StudentId,GETUTCDATE() AS [RegistrationDate]
FROM Students CROSS JOIN Courses ORDER BY NEWID()

GO