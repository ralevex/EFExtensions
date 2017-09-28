CREATE TABLE [dbo].[Registrations] (
    [CourseId]         UNIQUEIDENTIFIER NOT NULL,
    [StudentId]        UNIQUEIDENTIFIER NOT NULL,
    [RegistrationDate] DATE             CONSTRAINT [DF_Registrations_RegistrationDate] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_Registrations] PRIMARY KEY CLUSTERED ([CourseId] ASC, [StudentId] ASC),
    CONSTRAINT [FK_Registrations_Courses] FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses] ([CourseId]),
    CONSTRAINT [FK_Registrations_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([StudentId])
);

