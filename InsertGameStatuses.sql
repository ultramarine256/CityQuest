-- inserting standart statuses for Games
USE [YOUR_DATABASE_NAME]
GO

INSERT INTO [dbo].[GameStatus]
           ([Name]
           ,[Description]
		   .[NextAllowedStatusNames]
           ,[IsDefault]
           ,[IsDeleted]
           ,[CreationTime])
     VALUES
           ('GameStatus_Planned', 'Is used for games that are planned.', 'GameStatus_InProgress', 1, 0, '2015-12-12 12:12:12'),
		   ('GameStatus_InProgress', 'Is used for games that are in progress.', 'GameStatus_Completed,GameStatus_Paused', 0, 0, '2015-12-12 12:12:12'),
		   ('GameStatus_Paused', 'Is used for games that are paused.', 'GameStatus_InProgress', 0, 0, '2015-12-12 12:12:12'),
		   ('GameStatus_Completed', 'Is used for games that are completed.', Null, 0, 0, '2015-12-12 12:12:12')
GO