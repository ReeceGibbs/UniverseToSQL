USE [AUTOZONE.SYNC]
GO

/****** Object:  StoredProcedure [dbo].[BulkInsert]    Script Date: 2021/09/01 08:02:39 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE proc [dbo].[BulkInsert]

	@destinationTable varchar(128),
	@sourcePath varchar(256),
	@rowNum varchar(16)

as
begin

	create table #tempImport
	(
		Id varchar(512),
		Array varchar(max)
	)
	
	declare 
		@stageCommand nvarchar(max),
		@insertCommand nvarchar(max)

	set @stageCommand = 
		'bulk insert #tempImport
		 from ''' + @sourcePath + '''
		 WITH(FIELDTERMINATOR = ''0x09'', ROWTERMINATOR = ''0x0A'', LASTROW = ' + @rowNum + ', CODEPAGE = 1252)'

	set @insertCommand = 
		 'insert into [' + @destinationTable + ']
		 select Id, Array from #tempImport'

	exec sp_executesql @stageCommand

	exec sp_executesql @insertCommand
end
GO


