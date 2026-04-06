using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.Script.Serialization;

namespace prjLibrarySystem
{
    /// <summary>
    /// Summary description for SearchSuggestions
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [ScriptService]
    public class SearchSuggestions : System.Web.Services.WebService
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["LibraryDB"].ConnectionString;

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<SearchSuggestion> GetBookSuggestions(string query)
        {
            var suggestions = new List<SearchSuggestion>();
            
            System.Diagnostics.Debug.WriteLine("GetBookSuggestions called with query: " + query);
            
            if (string.IsNullOrEmpty(query) || query.Length < 2)
            {
                System.Diagnostics.Debug.WriteLine("Query too short or empty");
                return suggestions;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine("Database connection opened");
                    
                    string sql = @"
                        SELECT TOP 10 ISBN, Title, Author 
                        FROM Books 
                        WHERE (Title LIKE @Query + '%' OR Author LIKE @Query + '%' OR ISBN LIKE @Query + '%')
                        ORDER BY Title";
                    
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Query", query);
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                suggestions.Add(new SearchSuggestion
                                {
                                    Value = reader["Title"].ToString(),
                                    DisplayText = $"<strong>{reader["Title"]}</strong> by {reader["Author"]}",
                                    SubText = $"ISBN: {reader["ISBN"]}"
                                });
                            }
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"Found {suggestions.Count} suggestions");
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error getting book suggestions: {ex.Message}");
            }

            return suggestions;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<SearchSuggestion> GetMemberSuggestions(string query)
        {
            var suggestions = new List<SearchSuggestion>();
            
            if (string.IsNullOrEmpty(query) || query.Length < 2)
                return suggestions;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT TOP 10 MemberID, FullName, Email, Username 
                        FROM Members 
                        WHERE (FullName LIKE @Query + '%' OR Email LIKE @Query + '%' OR Username LIKE @Query + '%' OR MemberID LIKE @Query + '%')
                        ORDER BY FullName";
                    
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Query", query);
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                suggestions.Add(new SearchSuggestion
                                {
                                    Value = reader["FullName"].ToString(),
                                    DisplayText = $"<strong>{reader["FullName"]}</strong> - {reader["Email"]}",
                                    SubText = $"ID: {reader["MemberID"]}"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error getting member suggestions: {ex.Message}");
            }

            return suggestions;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<SearchSuggestion> GetTransactionSuggestions(string query)
        {
            var suggestions = new List<SearchSuggestion>();
            
            if (string.IsNullOrEmpty(query) || query.Length < 2)
                return suggestions;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT TOP 10 LoanID, b.Title, m.FullName 
                        FROM BorrowTransactions bt
                        JOIN Books b ON bt.ISBN = b.ISBN
                        JOIN Members m ON bt.MemberID = m.MemberID
                        WHERE (bt.LoanID LIKE @Query + '%' OR b.Title LIKE @Query + '%' OR m.FullName LIKE @Query + '%')
                        ORDER BY bt.LoanDate DESC";
                    
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Query", query);
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                suggestions.Add(new SearchSuggestion
                                {
                                    Value = reader["LoanID"].ToString(),
                                    DisplayText = $"<strong>{reader["LoanID"]}</strong> - {reader["Title"]}",
                                    SubText = $"Member: {reader["FullName"]}"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error getting transaction suggestions: {ex.Message}");
            }

            return suggestions;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<SearchSuggestion> GetAuditSuggestions(string query)
        {
            var suggestions = new List<SearchSuggestion>();
            
            if (string.IsNullOrEmpty(query) || query.Length < 2)
                return suggestions;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // User suggestions
                    string userSql = @"
                        SELECT DISTINCT Username 
                        FROM AuditLogs 
                        WHERE Username LIKE @Query + '%'
                        ORDER BY Username";
                    
                    using (SqlCommand cmd = new SqlCommand(userSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Query", query);
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                suggestions.Add(new SearchSuggestion
                                {
                                    Value = reader["Username"].ToString(),
                                    DisplayText = $"<strong>{reader["Username"]}</strong> - User actions",
                                    SubText = "User"
                                });
                            }
                        }
                    }

                    // Action suggestions
                    string actionSql = @"
                        SELECT DISTINCT Action 
                        FROM AuditLogs 
                        WHERE Action LIKE @Query + '%'
                        ORDER BY Action";
                    
                    using (SqlCommand cmd = new SqlCommand(actionSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Query", query);
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                suggestions.Add(new SearchSuggestion
                                {
                                    Value = reader["Action"].ToString(),
                                    DisplayText = $"<strong>{reader["Action"]}</strong> - Action",
                                    SubText = "Action"
                                });
                            }
                        }
                    }

                    // Table suggestions
                    string tableSql = @"
                        SELECT DISTINCT TableName 
                        FROM AuditLogs 
                        WHERE TableName LIKE @Query + '%'
                        ORDER BY TableName";
                    
                    using (SqlCommand cmd = new SqlCommand(tableSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Query", query);
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                suggestions.Add(new SearchSuggestion
                                {
                                    Value = reader["TableName"].ToString(),
                                    DisplayText = $"<strong>{reader["TableName"]}</strong> - Table operations",
                                    SubText = "Table"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error getting audit suggestions: {ex.Message}");
            }

            // Limit to top 10 results
            if (suggestions.Count > 10)
                suggestions = suggestions.GetRange(0, 10);

            return suggestions;
        }
    }

    public class SearchSuggestion
    {
        public string Value { get; set; }
        public string DisplayText { get; set; }
        public string SubText { get; set; }
    }
}
