ALTER DATABASE [$(DatabaseName)]
    ADD LOG FILE (NAME = [FormProcessor_log], FILENAME = '$(DefaultLogPath)$(DatabaseName)_log.LDF', MAXSIZE = 2097152 MB, FILEGROWTH = 10 %);

