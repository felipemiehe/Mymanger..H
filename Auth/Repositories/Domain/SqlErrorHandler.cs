﻿using Auth.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

public class SqlErrorHandler
{
    public IActionResult HandleSqlException(Exception ex)
    {
        if (ex.InnerException is SqlException sqlException && sqlException.Number == 2601)
        {
            // Número 2601 é a exceção específica para violação de restrição única no SQL Server
            var errorMessage = sqlException.Message;

            // Extrai o valor duplicado da mensagem de erro
            var startIndex = errorMessage.IndexOf("(") + 1;
            var endIndex = errorMessage.IndexOf(")");
            var valorDuplicado = errorMessage.Substring(startIndex, endIndex - startIndex);

            return new BadRequestObjectResult(new ResponseDTO { Status = "Error", Message = $"O valor '{valorDuplicado}' já está em uso." });
        }
        return new StatusCodeResult(500);
    }
}