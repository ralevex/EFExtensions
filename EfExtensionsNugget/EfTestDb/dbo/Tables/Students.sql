CREATE TABLE [dbo].[Students] (
    [StudentId] UNIQUEIDENTIFIER CONSTRAINT [DF_Students_StudentId] DEFAULT (newid()) NOT NULL,
    [FirstName] NVARCHAR (16)    NOT NULL,
    [LastName]  NVARCHAR (32)    NOT NULL,
    CONSTRAINT [PK_Students] PRIMARY KEY CLUSTERED ([StudentId] ASC)
);

