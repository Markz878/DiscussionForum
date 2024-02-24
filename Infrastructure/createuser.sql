CREATE USER [discussionforum-identity] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [discussionforum-identity]; 
ALTER ROLE db_datawriter ADD MEMBER [discussionforum-identity];