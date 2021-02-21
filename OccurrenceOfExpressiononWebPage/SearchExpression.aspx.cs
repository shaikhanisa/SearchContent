using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OccurrenceOfExpressiononWebPage
{
    public partial class SearchExpression : System.Web.UI.Page
    {
        static int count = 0;        
        static string message = null;
        static List<ReportData> reportDetails = new List<ReportData>();
        //Database details need to be entered in config file before running the code.
        static SqlConnection sqlconn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SQLDataConnection"].ConnectionString);
        protected void Page_Load(object sender, EventArgs e)
        {
            if(!IsPostBack)
            GridViewCoulums();
        }    

        /// <summary>
        /// Tgis method is used to extract all src and href links from a HTML string.
        /// </summary>
        /// <param name="html">The html source</param>
        /// <returns>A list of links - these will be all links including javascript ones.</returns>
        public class LinkExtractor
        {
            
            public static List<string> Extract(string html)
            {
                List<string> list = new List<string>();

                Regex regex = new Regex("(?:href|src)=[\"|']?(.*?)[\"|'|>]+", RegexOptions.Singleline | RegexOptions.CultureInvariant);
                if (regex.IsMatch(html))
                {
                    foreach (Match match in regex.Matches(html))
                    {
                        list.Add(match.Groups[1].Value);
                    }
                }

                return list;
            }
        }
     

        /// <summary>
        /// This web method is called when users hits the Start button containing the parameters given by the user.
        /// </summary>
        /// <param name="details">Json value conatining url and search value</param>
        /// <returns>Status of the result to User</returns>
        [System.Web.Services.WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static string StartSearch(SearchRequest details)
        {
            try
            {
                count = 0;                

                WebClient client = new WebClient();
                using (Stream data = client.OpenRead(details.urlofWebpage))
                {
                    using (StreamReader reader = new StreamReader(data))
                    {
                        string content = reader.ReadToEnd();
                        List<string> list = LinkExtractor.Extract(content);
                        string result = list.FirstOrDefault(x => x == details.urlofWebpage);
                        if (result == null)
                        {
                            SearchContent(details.urlofWebpage, details.searchExpression);
                        }
                        foreach (var link in list)
                        {
                            //--------------------------------                                              
                            SearchContent(link, details.searchExpression);
                            //------------------------------------

                            Console.WriteLine(link);
                        }

                        List<string> valueToInsert = new List<string>();

                        valueToInsert.Add(DateTime.UtcNow.ToString("dd-MM-yyyy"));
                        valueToInsert.Add(details.urlofWebpage);
                        valueToInsert.Add(details.searchExpression);
                        valueToInsert.Add(count.ToString());

                        valueToInsert.ToArray();

                        
                        String query = "INSERT INTO [dbo].[SearchReport] (Date,URL,Printout,NoOfHits) VALUES (@Date,@URL,@Printout, @NoOfHits)";

                        SqlCommand cmd = new SqlCommand(query, sqlconn);

                        cmd.Parameters.AddWithValue("@Date", DateTime.UtcNow.ToString("dd-MM-yyyy"));
                        cmd.Parameters.AddWithValue("@URL", details.urlofWebpage);
                        cmd.Parameters.AddWithValue("@Printout", details.searchExpression);
                        cmd.Parameters.AddWithValue("@NoOfHits", count);

                        sqlconn.Open();
                        cmd.ExecuteNonQuery();

                        message = "Success";
                        sqlconn.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                sqlconn.Close();
                message = "Failure";
            }
            return message;
        }
        /// <summary> 
        ///This method is used to search for content on the Website in each link found on the main page
        /// </summary>
        /// <param name="urlofWebpage"></param>
        /// <param name="searchExpression"></param>
        public static void SearchContent(string urlofWebpage, string searchExpression)
        {         
            try
            {
                WebClient client = new WebClient();
                using (Stream data = client.OpenRead(urlofWebpage))
                {
                    using (StreamReader reader = new StreamReader(data))
                    {
                        string content = reader.ReadToEnd();
                        string pattern = searchExpression;
                        MatchCollection matches = Regex.Matches(content, pattern);
                        foreach (Match match in matches)
                        {
                            GroupCollection groups = match.Groups;                            
                            count++;

                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exceptopn occured " + ex.Message);
            }
        }
        
        /// <summary>
        /// Method used to Generate report from SQL Database
        /// </summary>
        /// <returns>List of vlaues from Database</returns>
        [System.Web.Services.WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static ReportData[] GetReport()
        {
            try
            {
                DataTable dt = new DataTable();                             
                SqlCommand cmd = new SqlCommand("Select * from [dbo].[SearchReport]", sqlconn);
                sqlconn.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                foreach (DataRow drow in dt.Rows)
                {
                    ReportData reportData = new ReportData();
                    reportData.Date = drow["Date"].ToString();
                    reportData.URL = drow["URL"].ToString();
                    reportData.Printout = drow["Printout"].ToString();
                    reportData.NoOfHits = drow["NoOfHits"].ToString();
                    reportDetails.Add(reportData);
                }
                sqlconn.Close();
            }
            catch(Exception ex)
            {
                message = "Failure";
                sqlconn.Close();
            }
            return reportDetails.ToArray();

        }

        /// <summary>
        /// Medthod to add rows in Gridview
        /// </summary>
        public void GridViewCoulums()
        {
            DataTable reportTable = new DataTable();
            reportTable.Columns.Add("Date");
            reportTable.Columns.Add("URL");
            reportTable.Columns.Add("Printout");
            reportTable.Columns.Add("NoOfHits");
            reportTable.Rows.Add();
            reportGridView.DataSource = reportTable;
            reportGridView.DataBind();
            reportGridView.Rows[0].Visible = false;
        }

       
        public class ReportData
        {
            public string Date { get; set; }
            public string URL { get; set; }
            public string Printout { get; set; }
            public string NoOfHits { get; set; }

        }

        public class SearchRequest
        {
            public string urlofWebpage { get; set; }
            public string searchExpression { get; set; }
        }
    }
}