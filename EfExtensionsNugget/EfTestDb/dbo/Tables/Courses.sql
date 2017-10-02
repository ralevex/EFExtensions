CREATE TABLE [dbo].[Courses] (
    [CourseId]   UNIQUEIDENTIFIER CONSTRAINT [DF_Courses_CourseId] DEFAULT (newid()) NOT NULL,
    [CourseName] NVARCHAR (32)    NOT NULL,
    CONSTRAINT [PK_Courses] PRIMARY KEY CLUSTERED ([CourseId] ASC)
);

