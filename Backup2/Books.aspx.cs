using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using prjLibrarySystem.Models;

namespace prjLibrarySystem
{
    public partial class Books : System.Web.UI.Page
    {
        private string SearchTerm
        {
            get { return ViewState["SearchTerm"] as string ?? ""; }
            set { ViewState["SearchTerm"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) { Response.Redirect("Login.aspx"); return; }

            string role = Session["Role"]?.ToString();
            if (role != "Admin" && role != "Super Admin")
            {
                Response.Redirect("MemberDashboard.aspx");
                return;
            }

            litSidebar.Text = SidebarHelper.GetSidebar(role, "books");

            if (!IsPostBack)
            {
                LoadCategories();
                LoadBooks();
            }
        }

        private void LoadBooks()
        {
            try
            {
                string query = @"
                    SELECT
                        v.ISBN, v.Title, v.Author,
                        c.CategoryName,
                        v.TotalCopies, v.AvailableCopies,
                        b.Description, b.CategoryID
                    FROM vwBookAvailability v
                    INNER JOIN tblBooks b ON b.ISBN = v.ISBN
                    LEFT  JOIN tblCategories c ON c.CategoryID = b.CategoryID
                    WHERE 1=1";

                var parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    query += " AND (v.Title LIKE @Search OR v.Author LIKE @Search OR v.ISBN LIKE @Search OR b.Description LIKE @Search)";
                    parameters.Add(new SqlParameter("@Search", "%" + SearchTerm + "%"));
                }

                if (!string.IsNullOrEmpty(ddlCategory.SelectedValue))
                {
                    query += " AND b.CategoryID = @CategoryID";
                    parameters.Add(new SqlParameter("@CategoryID", Convert.ToInt32(ddlCategory.SelectedValue)));
                }
                if (!string.IsNullOrEmpty(ddlAvailability.SelectedValue))
                {
                    if (ddlAvailability.SelectedValue == "Available")
                        query += " AND v.AvailableCopies > 0";
                    else if (ddlAvailability.SelectedValue == "OutOfStock")
                        query += " AND v.AvailableCopies = 0";
                }

                query += " ORDER BY v.Title";
                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters.ToArray());
                gvBooks.DataSource = dt;
                gvBooks.DataBind();
                txtSearch.Text = SearchTerm;
            }
            catch (Exception ex)
            {
                gvBooks.DataSource = null;
                gvBooks.DataBind();
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    $"alert('Error loading books: {ex.Message}');", true);
            }
        }

        private void LoadCategories()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(@"
        SELECT CategoryID, CategoryName 
        FROM tblCategories 
        WHERE IsActive = 1
        ORDER BY CategoryName", null);

            // FILTER DROPDOWN
            ddlCategory.DataSource = dt;
            ddlCategory.DataTextField = "CategoryName";
            ddlCategory.DataValueField = "CategoryID";
            ddlCategory.DataBind();
            ddlCategory.Items.Insert(0, new ListItem("All Categories", ""));

            // ADD / EDIT DROPDOWN
            ddlBookCategory.DataSource = dt;
            ddlBookCategory.DataTextField = "CategoryName";
            ddlBookCategory.DataValueField = "CategoryID";
            ddlBookCategory.DataBind();
            ddlBookCategory.Items.Insert(0, new ListItem("Select Category", ""));
        }

        protected void btnSearch_Click(object sender, EventArgs e) { SearchTerm = txtSearch.Text.Trim(); gvBooks.PageIndex = 0; LoadBooks(); }
        protected void ddlCategory_SelectedIndexChanged(object sender, EventArgs e) { gvBooks.PageIndex = 0; LoadBooks(); }
        protected void ddlAvailability_SelectedIndexChanged(object sender, EventArgs e) { gvBooks.PageIndex = 0; LoadBooks(); }
        protected void gvBooks_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvBooks.PageIndex = e.NewPageIndex; LoadBooks(); }

        protected void gvBooks_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string isbn = e.CommandArgument.ToString();
            switch (e.CommandName)
            {
                case "EditBook": LoadBookForEdit(isbn); break;
                case "DeleteBook": DeleteBook(isbn); break;
                case "ViewDetails": ViewBookDetails(isbn); break;
                case "BorrowBook": BorrowBook(isbn); break;
            }
        }

        // FIX 1: BorrowBook method — closing brace was misplaced (after the null-check return),
        // leaving the rest of the method body floating outside any method.
        // Also fixed: 'settings' and 'userId' were undeclared. They are now properly retrieved.
        private void BorrowBook(string isbn)
        {
            string memberType = Session["MemberType"]?.ToString();

            if (string.IsNullOrEmpty(memberType))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert",
                    "alert('Session expired. Please login again.');", true);
                return;
            }

            // FIX 2: 'userId' was used but never declared — get it from Session.
            string userId = Session["UserID"]?.ToString();

            // FIX 3: 'settings' was used but never declared — call GetBorrowSettings().
            dynamic settings = GetBorrowSettings();

            int maxAllowed = 0;
            int duration = 0;

            if (memberType == "Student")
            {
                maxAllowed = settings.StudentMaxBooks;
                duration = settings.StudentBorrowDays;
            }
            else
            {
                maxAllowed = settings.TeacherMaxBooks;
                duration = settings.TeacherBorrowDays;
            }

            int currentBorrowed = GetCurrentBorrowCount(userId);

            if (currentBorrowed >= maxAllowed)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert",
                    "alert('Borrow limit reached');", true);
                return;
            }

            DateTime dueDate = DateTime.Now.AddDays(duration);

            SaveBorrowTransaction(userId, isbn, dueDate);

            ClientScript.RegisterStartupScript(this.GetType(), "alert",
                "alert('Book borrowed successfully');", true);
        }

        // FIX 4: Added missing helper methods that BorrowBook() calls.
        private int GetCurrentBorrowCount(string userId)
        {
            object result = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM tblTransactions WHERE UserID=@UserID AND Status='Active'",
                new SqlParameter[] { new SqlParameter("@UserID", userId) });
            return result != null ? Convert.ToInt32(result) : 0;
        }

        private void SaveBorrowTransaction(string userId, string isbn, DateTime dueDate)
        {
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO tblTransactions (UserID, ISBN, BorrowDate, DueDate, Status)
                VALUES (@UserID, @ISBN, GETDATE(), @DueDate, 'Active')",
                new SqlParameter[]
                {
                    new SqlParameter("@UserID",  userId),
                    new SqlParameter("@ISBN",    isbn),
                    new SqlParameter("@DueDate", dueDate)
                });
        }

        private void LoadBookForEdit(string isbn)
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(
                    "SELECT ISBN, Title, Author, CategoryID, TotalCopies, Description FROM tblBooks WHERE ISBN=@ISBN",
                    new SqlParameter[] { new SqlParameter("@ISBN", isbn) });

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    hfBookId.Value = row["ISBN"].ToString();
                    txtISBN.Text = row["ISBN"].ToString();
                    txtISBN.Enabled = false;
                    txtTitle.Text = row["Title"].ToString();
                    txtAuthor.Text = row["Author"].ToString();

                    if (row["CategoryID"] != DBNull.Value)
                    {
                        string catId = row["CategoryID"].ToString();
                        if (ddlBookCategory.Items.FindByValue(catId) != null)
                        {
                            ddlBookCategory.SelectedValue = catId;
                        }
                    }

                    txtTotalCopies.Text = row["TotalCopies"].ToString();
                    txtDescription.Text = row["Description"]?.ToString() ?? "";
                    lblModalTitle.Text = "Edit Book";
                    ClientScript.RegisterStartupScript(GetType(), "showModal", "<script>showBookModal();</script>");
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert",
                    $"alert('Error loading book: {ex.Message}');", true);
            }
        }

        private void DeleteBook(string isbn)
        {
            try
            {
                int active = Convert.ToInt32(DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM tblTransactions WHERE ISBN=@ISBN AND Status='Active'",
                    new SqlParameter[] { new SqlParameter("@ISBN", isbn) }));

                if (active > 0)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert",
                        "alert('Cannot delete a book that is currently borrowed.');", true);
                    return;
                }

                DatabaseHelper.ExecuteNonQuery("DELETE FROM tblBooks WHERE ISBN=@ISBN",
                    new SqlParameter[] { new SqlParameter("@ISBN", isbn) });
                DatabaseHelper.WriteAuditLog(Session["UserID"]?.ToString(), Session["FullName"]?.ToString(),
                    "DELETE_BOOK", "tblBooks", isbn);

                LoadBooks();
                ScriptManager.RegisterStartupScript(this, GetType(), "success",
                    "alert('Book deleted successfully.');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert",
                    $"alert('Error deleting book: {ex.Message}');", true);
            }
        }

        private void ViewBookDetails(string isbn)
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(@"
                    SELECT v.ISBN, v.Title, v.Author, v.TotalCopies, v.AvailableCopies,
                           c.CategoryName, b.Description
                    FROM vwBookAvailability v
                    INNER JOIN tblBooks b ON b.ISBN = v.ISBN
                    LEFT  JOIN tblCategories c ON c.CategoryID = b.CategoryID
                    WHERE v.ISBN = @ISBN",
                    new SqlParameter[] { new SqlParameter("@ISBN", isbn) });

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    string Esc(string f) => (row[f]?.ToString() ?? "").Replace("'", "\\'");
                    string script = $@"
                        document.getElementById('viewISBN').innerText='{Esc("ISBN")}';
                        document.getElementById('viewTitle').innerText='{Esc("Title")}';
                        document.getElementById('viewAuthor').innerText='{Esc("Author")}';
                        document.getElementById('viewCategory').innerText='{Esc("CategoryName")}';
                        document.getElementById('viewTotalCopies').innerText='{row["TotalCopies"]}';
                        document.getElementById('viewAvailableCopies').innerText='{row["AvailableCopies"]}';
                        document.getElementById('viewDescription').innerText='{(row["Description"] ?? "").ToString().Replace("'", "\\'").Replace("\r\n", "\\n")}';
                        showViewBookModal();";
                    ClientScript.RegisterStartupScript(GetType(), "showViewModal", "<script>" + script + "</script>");
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert",
                    $"alert('Error loading book details: {ex.Message}');", true);
            }
        }

        protected void btnSaveBook_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtISBN.Text) || string.IsNullOrEmpty(txtTitle.Text) ||
                    string.IsNullOrEmpty(txtAuthor.Text) || string.IsNullOrEmpty(ddlBookCategory.SelectedValue) ||
                    string.IsNullOrEmpty(txtTotalCopies.Text))
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "validation",
                        "alert('Please fill in all required fields.');", true);
                    return;
                }

                int totalCopies = Convert.ToInt32(txtTotalCopies.Text);
                int categoryId = Convert.ToInt32(ddlBookCategory.SelectedValue);

                string adminId = Session["UserID"]?.ToString() ?? "";
                string adminName = Session["FullName"]?.ToString() ?? "";

                if (string.IsNullOrEmpty(hfBookId.Value))
                {
                    DatabaseHelper.ExecuteNonQuery(@"
                        INSERT INTO tblBooks (ISBN, Title, Author, CategoryID, TotalCopies, Description, DateAdded, UpdatedAt)
                        VALUES (@ISBN, @Title, @Author, @CategoryID, @TotalCopies, @Description, GETDATE(), GETDATE())",
                        new SqlParameter[]
                        {
                            new SqlParameter("@ISBN",        txtISBN.Text),
                            new SqlParameter("@Title",       txtTitle.Text),
                            new SqlParameter("@Author",      txtAuthor.Text),
                            new SqlParameter("@CategoryID",  categoryId),
                            new SqlParameter("@TotalCopies", totalCopies),
                            new SqlParameter("@Description", txtDescription.Text)
                        });
                    DatabaseHelper.WriteAuditLog(adminId, adminName, "ADD_BOOK", "tblBooks", txtISBN.Text);
                    ScriptManager.RegisterStartupScript(this, GetType(), "success", "alert('Book added successfully.');", true);
                }
                else
                {
                    DatabaseHelper.ExecuteNonQuery(@"
                        UPDATE tblBooks
                        SET Title=@Title, Author=@Author, CategoryID=@CategoryID,
                            TotalCopies=@TotalCopies, Description=@Description, UpdatedAt=GETDATE()
                        WHERE ISBN=@ISBN",
                        new SqlParameter[]
                        {
                            new SqlParameter("@ISBN",        txtISBN.Text),
                            new SqlParameter("@Title",       txtTitle.Text),
                            new SqlParameter("@Author",      txtAuthor.Text),
                            new SqlParameter("@CategoryID",  categoryId),
                            new SqlParameter("@TotalCopies", totalCopies),
                            new SqlParameter("@Description", txtDescription.Text)
                        });
                    DatabaseHelper.WriteAuditLog(adminId, adminName, "EDIT_BOOK", "tblBooks", hfBookId.Value);
                    ScriptManager.RegisterStartupScript(this, GetType(), "success", "alert('Book updated successfully.');", true);
                }

                SearchTerm = "";
                LoadBooks();
                ScriptManager.RegisterStartupScript(this, GetType(), "hideModal", "hideBookModal();", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert",
                    $"alert('Error saving book: {ex.Message}');", true);
            }
        }

        [WebMethod]
        public static string SaveBorrowSettings(int studentMaxBooks, int studentBorrowDays,
                                                 int teacherMaxBooks, int teacherBorrowDays)
        {
            try
            {
                if (studentMaxBooks < 1 || studentBorrowDays < 1 || teacherMaxBooks < 1 || teacherBorrowDays < 1)
                    return "All values must be positive numbers.";

                DataTable oldDt = DatabaseHelper.ExecuteQuery(
                    "SELECT MemberType, SettingKey, SettingValue FROM tblBorrowPolicies", new SqlParameter[0]);
                string oldValues = "";
                foreach (DataRow r in oldDt.Rows)
                    oldValues += $"{r["MemberType"]}.{r["SettingKey"]}={r["SettingValue"]}; ";
                string newValues =
                    $"Student.MaxBorrowedBooks={studentMaxBooks};Student.BorrowDuration={studentBorrowDays};" +
                    $"Teacher.MaxBorrowedBooks={teacherMaxBooks};Teacher.BorrowDuration={teacherBorrowDays};";

                ExecUpdateSetting("Student", "MaxBorrowedBooks", studentMaxBooks);
                ExecUpdateSetting("Student", "BorrowDuration", studentBorrowDays);
                ExecUpdateSetting("Teacher", "MaxBorrowedBooks", teacherMaxBooks);
                ExecUpdateSetting("Teacher", "BorrowDuration", teacherBorrowDays);

                DatabaseHelper.WriteAuditLog("System", "Admin", "EDIT_BORROW_SETTINGS", "tblBorrowPolicies",
                    null, oldValues.TrimEnd(), newValues.TrimEnd());
                return "OK";
            }
            catch (Exception ex) { return "Error: " + ex.Message; }
        }

        private static void ExecUpdateSetting(string memberType, string settingKey, int value)
        {
            DatabaseHelper.ExecuteNonQuery(@"
                UPDATE tblBorrowPolicies
                SET    SettingValue=@Value, LastUpdatedAt=GETDATE()
                WHERE  MemberType=@MemberType AND SettingKey=@SettingKey",
                new SqlParameter[]
                {
                    new SqlParameter("@Value",      value),
                    new SqlParameter("@MemberType", memberType),
                    new SqlParameter("@SettingKey", settingKey)
                });
        }

        [WebMethod]
        public static object GetBookData(string isbn)
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(@"
                    SELECT v.ISBN, v.Title, v.Author, v.TotalCopies, v.AvailableCopies,
                           b.CategoryID, c.CategoryName, b.Description
                    FROM vwBookAvailability v
                    INNER JOIN tblBooks b ON b.ISBN = v.ISBN
                    LEFT  JOIN tblCategories c ON c.CategoryID = b.CategoryID
                    WHERE v.ISBN = @ISBN",
                    new SqlParameter[] { new SqlParameter("@ISBN", isbn) });

                if (dt.Rows.Count == 0) return null;
                DataRow r = dt.Rows[0];
                return new
                {
                    ISBN = r["ISBN"].ToString(),
                    Title = r["Title"].ToString(),
                    Author = r["Author"].ToString(),
                    CategoryID = r["CategoryID"] != DBNull.Value ? Convert.ToInt32(r["CategoryID"]) : 0,
                    CategoryName = r["CategoryName"]?.ToString() ?? "",
                    TotalCopies = Convert.ToInt32(r["TotalCopies"]),
                    AvailableCopies = Convert.ToInt32(r["AvailableCopies"]),
                    Description = r["Description"]?.ToString() ?? ""
                };
            }
            catch { return null; }
        }

        private void ClearBookForm()
        {
            txtISBN.Text = ""; txtISBN.Enabled = true; txtTitle.Text = ""; txtAuthor.Text = "";
            ddlBookCategory.SelectedIndex = 0; txtTotalCopies.Text = ""; txtDescription.Text = "";
        }

        private dynamic GetBorrowSettings()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(
                "SELECT MemberType, SettingKey, SettingValue FROM tblBorrowPolicies",
                null);

            int stuMax = 0, stuDays = 0, tchMax = 0, tchDays = 0;

            foreach (DataRow r in dt.Rows)
            {
                string type = r["MemberType"].ToString();
                string key = r["SettingKey"].ToString();
                int value = Convert.ToInt32(r["SettingValue"]);

                if (type == "Student" && key == "MaxBorrowedBooks") stuMax = value;
                if (type == "Student" && key == "BorrowDuration") stuDays = value;
                if (type == "Teacher" && key == "MaxBorrowedBooks") tchMax = value;
                if (type == "Teacher" && key == "BorrowDuration") tchDays = value;
            }

            return new
            {
                StudentMaxBooks = stuMax,
                StudentBorrowDays = stuDays,
                TeacherMaxBooks = tchMax,
                TeacherBorrowDays = tchDays
            };
        }

        [WebMethod]
        public static List<object> GetSearchSuggestions(string query)
        {
            var suggestions = new List<object>();
            
            if (string.IsNullOrEmpty(query) || query.Length < 2)
                return suggestions;

            try
            {
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["LibraryDB"].ConnectionString;
                
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
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
                                suggestions.Add(new 
                                {
                                    Value = reader["Title"].ToString(),
                                    DisplayText = $"<strong>{reader["Title"]}</strong> by {reader["Author"]}",
                                    SubText = $"ISBN: {reader["ISBN"]}"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting book suggestions: {ex.Message}");
            }

            return suggestions;
        }
    }
}