using prjLibrarySystem.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace prjLibrarySystem
{
    public partial class Settings : System.Web.UI.Page
    {
        // ================== ViewState Properties ==================
        string CurrentTable
        {
            get => ViewState["CurrentTable"]?.ToString();
            set => ViewState["CurrentTable"] = value;
        }

        string CurrentField
        {
            get => ViewState["CurrentField"]?.ToString();
            set => ViewState["CurrentField"] = value;
        }

        string CurrentID
        {
            get => ViewState["CurrentID"]?.ToString();
            set => ViewState["CurrentID"] = value;
        }

        // ================== Page Load ==================
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) { Response.Redirect("Login.aspx"); return; }

            string role = Session["Role"]?.ToString();

            // Settings page is Admin-only (Super Admin uses a separate portal)
            if (role != "Admin")
            {
                Response.Redirect("MemberDashboard.aspx");
                return;
            }

            litSidebar.Text = SidebarHelper.GetSidebar(role, "settings");

            if (!IsPostBack)
            {
                LoadPolicyCards();
                LoadCategories();
                LoadYearLevel();
                LoadCourses();
            }
        }

        // ================== LOAD DATA ==================
        void LoadPolicyCards()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(
                "SELECT MemberType, SettingKey, SettingValue FROM tblBorrowPolicies", null);

            foreach (DataRow r in dt.Rows)
            {
                string type = r["MemberType"].ToString();
                string key = r["SettingKey"].ToString();
                string val = r["SettingValue"].ToString();

                if (type == "Student" && key == "MaxBorrowedBooks") txtStuMax.Text = val;
                else if (type == "Student" && key == "BorrowDuration") txtStuDays.Text = val;
                else if (type == "Teacher" && key == "MaxBorrowedBooks") txtTchMax.Text = val;
                else if (type == "Teacher" && key == "BorrowDuration") txtTchDays.Text = val;
            }
        }

        void LoadCategories()
        {
            gvCategories.DataSource = DatabaseHelper.ExecuteQuery(
                "SELECT CategoryID, CategoryName, IsActive FROM tblCategories ORDER BY CategoryName", null);
            gvCategories.DataBind();
        }

        void LoadYearLevel()
        {
            gvYearLevel.DataSource = DatabaseHelper.ExecuteQuery(
                "SELECT YearLevelID, YearLevelName, IsActive FROM tblYearLevels ORDER BY YearLevelName", null);
            gvYearLevel.DataBind();
        }

        void LoadCourses()
        {
            gvCourse.DataSource = DatabaseHelper.ExecuteQuery(
                "SELECT CourseID, CourseName, IsActive FROM tblCourses ORDER BY CourseName", null);
            gvCourse.DataBind();
        }

        // ================== CATEGORY ==================
        protected void btnAddCategory_Click(object sender, EventArgs e)
        {
            // FIX: Added blank input guard before inserting
            if (string.IsNullOrWhiteSpace(txtCategory.Text))
            {
                ShowMessage("Category name cannot be empty.", "warning");
                return;
            }

            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO tblCategories(CategoryName, IsActive) VALUES(@name, 1)",
                new SqlParameter[] { new SqlParameter("@name", txtCategory.Text.Trim()) });

            txtCategory.Text = "";
            ShowMessage("Category added successfully.");
            LoadCategories();
        }

        protected void btnToggleCategory_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);
            DatabaseHelper.ExecuteNonQuery(@"
                UPDATE tblCategories
                SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                WHERE CategoryID = @id",
                new SqlParameter[] { new SqlParameter("@id", id) });

            LoadCategories();
        }

        protected void btnDeleteCategory_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);

            // FIX: Used ExecuteScalar instead of ExecuteQuery for COUNT check — more efficient
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM tblBooks WHERE CategoryID = @id",
                new SqlParameter[] { new SqlParameter("@id", id) }));

            if (count > 0)
            {
                ShowMessage("Cannot delete — this category is in use by one or more books.", "danger");
                return;
            }

            DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM tblCategories WHERE CategoryID = @id",
                new SqlParameter[] { new SqlParameter("@id", id) });

            ShowMessage("Category deleted.");
            LoadCategories();
        }

        // ================== YEAR LEVEL ==================
        protected void btnAddYear_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtYearLevel.Text))
            {
                ShowMessage("Year Level name cannot be empty.", "warning");
                return;
            }

            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO tblYearLevels(YearLevelName, IsActive) VALUES(@name, 1)",
                new SqlParameter[] { new SqlParameter("@name", txtYearLevel.Text.Trim()) });

            txtYearLevel.Text = "";
            ShowMessage("Year Level added successfully.");
            LoadYearLevel();
        }

        protected void btnToggleYear_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);
            DatabaseHelper.ExecuteNonQuery(@"
                UPDATE tblYearLevels
                SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                WHERE YearLevelID = @id",
                new SqlParameter[] { new SqlParameter("@id", id) });

            LoadYearLevel();
        }

        protected void btnDeleteYear_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);

            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM tblMembers WHERE YearLevelID = @id",
                new SqlParameter[] { new SqlParameter("@id", id) }));

            if (count > 0)
            {
                ShowMessage("Cannot delete — this year level is assigned to one or more members.", "danger");
                return;
            }

            DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM tblYearLevels WHERE YearLevelID = @id",
                new SqlParameter[] { new SqlParameter("@id", id) });

            ShowMessage("Year Level deleted.");
            LoadYearLevel();
        }

        // ================== COURSE ==================
        protected void btnAddCourse_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCourse.Text))
            {
                ShowMessage("Course name cannot be empty.", "warning");
                return;
            }

            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO tblCourses(CourseName, IsActive) VALUES(@name, 1)",
                new SqlParameter[] { new SqlParameter("@name", txtCourse.Text.Trim()) });

            txtCourse.Text = "";
            ShowMessage("Course added successfully.");
            LoadCourses();
        }

        protected void btnToggleCourse_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);
            DatabaseHelper.ExecuteNonQuery(@"
                UPDATE tblCourses
                SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                WHERE CourseID = @id",
                new SqlParameter[] { new SqlParameter("@id", id) });

            LoadCourses();
        }

        protected void btnDeleteCourse_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);

            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM tblMembers WHERE CourseID = @id",
                new SqlParameter[] { new SqlParameter("@id", id) }));

            if (count > 0)
            {
                ShowMessage("Cannot delete — this course is assigned to one or more members.", "danger");
                return;
            }

            DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM tblCourses WHERE CourseID = @id",
                new SqlParameter[] { new SqlParameter("@id", id) });

            ShowMessage("Course deleted.");
            LoadCourses();
        }

        // ================== EDIT BUTTONS ==================
        protected void btnEditCategory_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);
            CurrentTable = "tblCategories";
            CurrentField = "CategoryName";
            CurrentID = "CategoryID";
            LoadEditData(id);
        }

        protected void btnEditYear_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);
            CurrentTable = "tblYearLevels";
            CurrentField = "YearLevelName";
            CurrentID = "YearLevelID";
            LoadEditData(id);
        }

        protected void btnEditCourse_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);
            CurrentTable = "tblCourses";
            CurrentField = "CourseName";
            CurrentID = "CourseID";
            LoadEditData(id);
        }

        protected void btnSavePolicy_Click(object sender, EventArgs e)
        {
            int stuMax, stuDays, tchMax, tchDays;

            if (!int.TryParse(txtStuMax.Text, out stuMax) || stuMax < 1 ||
                !int.TryParse(txtStuDays.Text, out stuDays) || stuDays < 1 ||
                !int.TryParse(txtTchMax.Text, out tchMax) || tchMax < 1 ||
                !int.TryParse(txtTchDays.Text, out tchDays) || tchDays < 1)
            {
                ShowMessage("All fields are required and must be positive numbers.", "warning");
                return;
            }

            // Capture old values for audit log
            DataTable oldDt = DatabaseHelper.ExecuteQuery(
                "SELECT MemberType, SettingKey, SettingValue FROM tblBorrowPolicies", null);
            string oldValues = "";
            foreach (DataRow r in oldDt.Rows)
                oldValues += $"{r["MemberType"]}.{r["SettingKey"]}={r["SettingValue"]}; ";

            string newValues =
                $"Student.MaxBorrowedBooks={stuMax}; Student.BorrowDuration={stuDays}; " +
                $"Teacher.MaxBorrowedBooks={tchMax}; Teacher.BorrowDuration={tchDays};";

            UpdatePolicySetting("Student", "MaxBorrowedBooks", stuMax);
            UpdatePolicySetting("Student", "BorrowDuration", stuDays);
            UpdatePolicySetting("Teacher", "MaxBorrowedBooks", tchMax);
            UpdatePolicySetting("Teacher", "BorrowDuration", tchDays);

            string userId = Session["UserID"]?.ToString() ?? "System";
            string userName = Session["FullName"]?.ToString() ?? "Admin";
            DatabaseHelper.WriteAuditLog(userId, userName, "EDIT_BORROW_SETTINGS",
                "tblBorrowPolicies", null, oldValues.TrimEnd(), newValues.TrimEnd());

            ShowMessage("Borrow policy updated successfully.");
        }

        private void UpdatePolicySetting(string memberType, string settingKey, int value)
        {
            DatabaseHelper.ExecuteNonQuery(@"
                UPDATE tblBorrowPolicies
                SET    SettingValue = @Value, LastUpdatedAt = GETDATE()
                WHERE  MemberType = @MemberType AND SettingKey = @SettingKey",
                new SqlParameter[]
                {
                    new SqlParameter("@Value",      value),
                    new SqlParameter("@MemberType", memberType),
                    new SqlParameter("@SettingKey",  settingKey)
                });
        }

        // ================== LOAD EDIT DATA ==================
        void LoadEditData(int id)
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(
                $"SELECT {CurrentField} FROM {CurrentTable} WHERE {CurrentID} = @id",
                new SqlParameter[] { new SqlParameter("@id", id) });

            hfID.Value = id.ToString();
            txtEditValue.Text = dt.Rows[0][0].ToString();

            ScriptManager.RegisterStartupScript(this, GetType(),
                "Popup", "new bootstrap.Modal(document.getElementById('editModal')).show();", true);
        }

        // ================== UPDATE BUTTON ==================
        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hfID.Value) || string.IsNullOrEmpty(CurrentTable)
                || string.IsNullOrEmpty(CurrentField) || string.IsNullOrEmpty(CurrentID))
            {
                ShowMessage("Invalid operation. Please try again.", "danger");
                return;
            }

            // FIX: Added blank value guard for the edit field
            if (string.IsNullOrWhiteSpace(txtEditValue.Text))
            {
                ShowMessage("Value cannot be empty.", "warning");
                ScriptManager.RegisterStartupScript(this, GetType(), "ReopenModal", "new bootstrap.Modal(document.getElementById('editModal')).show();", true);
                return;
            }

            int id = Convert.ToInt32(hfID.Value);

            DatabaseHelper.ExecuteNonQuery(
                $"UPDATE {CurrentTable} SET {CurrentField} = @value WHERE {CurrentID} = @id",
                new SqlParameter[]
                {
                    new SqlParameter("@value", txtEditValue.Text.Trim()),
                    new SqlParameter("@id",    id)
                });

            ShowMessage("Updated successfully.");

            switch (CurrentTable)
            {
                case "tblCategories": LoadCategories(); break;
                case "tblYearLevels": LoadYearLevel(); break;
                case "tblCourses": LoadCourses(); break;
                case "tblBorrowPolicies": LoadPolicyCards(); break;
            }

            ScriptManager.RegisterStartupScript(this, GetType(),
                "CloseModal", "var m=bootstrap.Modal.getInstance(document.getElementById('editModal'));if(m)m.hide();", true);
        }

        // ================== SHOW MESSAGE ==================
        void ShowMessage(string msg, string type = "success")
        {
            lblMsg.Text = msg;
            lblMsg.CssClass = "alert alert-" + type;
        }
    }
}