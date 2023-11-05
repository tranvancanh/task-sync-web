using Newtonsoft.Json;
using OfficeOpenXml;
using SpreadsheetLight;
using System.Collections;
using System.Data;
using task_sync_web.Models;

namespace task_sync_web.Commons
{
    public class ExcelFile<T>
    {
        public static MemoryStream ExcelCreate(List<T> listData, bool autoFitCol = false, int startX = 1, int startY = 1)
        {
            if (startX < 1) { throw new System.Exception(); }
            if (startY < 1) { throw new System.Exception(); }
            try
            {
                MemoryStream ms = new MemoryStream();
                using (SLDocument sl = new SLDocument())
                {
                    SLStyle keyStyle = sl.CreateStyle();

                    // 太字
                    keyStyle.SetFontBold(true);

                    // ModelのProperty一覧を取得
                    var properties = Utils.GetModelProperties<T>();

                    // ヘッダー行目：ヘッダーをセット
                    for (var i = 0; i < properties.Count(); i++)
                    {
                        sl.SetCellStyle(startX, startY + i, keyStyle);
                        sl.SetCellValue(startX, startY + i, properties[i].DisplayName);
                    }

                    // 次行目～：値をセット
                    foreach(var data in listData)
                    {
                        startX++;
                        var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                        var dicts = (Dictionary<string, string>)JsonConvert.DeserializeObject(jsonString, typeof(Dictionary<string, string>));
                        var values = dicts.Values.ToArray();
                        for(var i = 0; i < values.Length; i++)
                        {
                            sl.SetCellValue(startX, startY + i, values[i]);
                        }
                    }

                    if (autoFitCol)
                    {
                        sl.AutoFitColumn(0, properties.Count);
                    }

                    sl.SaveAs(ms);
                }

                ms.Position = 0;

                return ms;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<MemoryStream> ExcelCreateAsync(string[] headerList, DataTable data, bool autoFitCol = false, int startX = 0, int startY = 0)
        {
            if (startX < 0) { throw new System.Exception("開始位置がおかしいです！"); }
            if (startY < 0) { throw new System.Exception("開始位置がおかしいです！"); }
            try
            {
                MemoryStream ms = new MemoryStream();
                using (SLDocument sl = new SLDocument())
                {
                    SLStyle keyStyle = sl.CreateStyle();

                    // 太字
                    keyStyle.SetFontBold(true);

                    // 1行目：ヘッダーをセット
                    for (int i = 0; i < headerList.Length; ++i)
                    {
                        sl.SetCellStyle(startX, startY, keyStyle);
                        sl.SetCellValue(startX, startY, headerList[i]);
                    }

                    startX = startX + 1;

                    // 2行目～：値をセット
                    if (data != null && data.Rows.Count > 0)
                    {
                        for (int row = 0; row < data.Rows.Count; row++)
                        {
                            for (int col = 0; col < headerList.Length; ++col)
                            {
                                sl.SetCellValue(startX, startY, Convert.ToString(data.Rows[row][col]));
                            }
                        }
                    }

                    if (autoFitCol)
                    {
                        sl.AutoFitColumn(0, headerList.Length);
                    }

                    sl.SaveAs(ms);

                }

                await Task.CompletedTask;

                return ms;

            }
            catch (Exception)
            {
                throw;
            }

        }

        public static async Task<DataTable> ReadExcelToDataTable(IFormFile formFile)
        {
            if (formFile == null || formFile.Length == 0)
            {
                throw new ArgumentException("File is not exist");
            }

            var dataTable = new DataTable();

            //Get file
            var newfile = new FileInfo(formFile.FileName);
            var fileExtension = newfile.Extension;

            //Check if file is an Excel File
            if (fileExtension.ToLower().EndsWith(".xlsx"))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await formFile.CopyToAsync(ms);
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (ExcelPackage package = new ExcelPackage(ms))
                    {
                        ExcelWorksheet workSheet = package.Workbook.Worksheets.FirstOrDefault();
                        ArgumentNullException.ThrowIfNull(workSheet);
                        // get number of rows and columns in the sheet
                        int totalRows = workSheet.Dimension.Rows;
                        int totalColumns = workSheet.Dimension.Columns;

                        dataTable = await ConvertToDataTable(workSheet);
                    }
                }
            }
            else
                throw new ArgumentException(ErrorMessages.EW1202);

            return dataTable;
        }

        public static DataTable ToWithFormat(DataTable dataTable)
        {
            var firstRow = dataTable.Rows[0].ItemArray.ToList();
            var properties = Utils.GetModelProperties<T>();
            var newDt = new DataTable();
            foreach (var propertie in properties)
            {
                newDt.Columns.Add(propertie.PropertyName, typeof(string));
            }

            var columnNames = newDt.Columns.Cast<DataColumn>()
                                 .Select(x => x.ColumnName)
                                 .ToList();

            for (var i = 1; i < dataTable.Rows.Count; i++)
            {
                var dtRow = newDt.NewRow();
                for (var j = 0; j < columnNames.Count; j++)
                {
                    var colName = columnNames[j];
                    dtRow[colName] = dataTable.Rows[i][j];
                }
                newDt.Rows.Add(dtRow);
            }

            return newDt;
        }

        private static async Task<DataTable> ConvertToDataTable(ExcelWorksheet oSheet)
        {
            var rowStart = oSheet.Dimension.Start.Row;
            var colStart = oSheet.Dimension.Start.Column;

            int totalRows = oSheet.Dimension.End.Row;
            int totalCols = oSheet.Dimension.End.Column;
            DataTable dataTable = new DataTable(oSheet.Name);
            DataRow dataRow = null;
            for (int i = 0; i < totalCols; i++)
            {
                var dtColumn = new DataColumn();
                dtColumn.ColumnName = i.ToString();
                dataTable.Columns.Add(dtColumn);
            }

            var allCellVal = oSheet.Cells.Value;
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(allCellVal);
            var arrayList = (ArrayList)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString, typeof(ArrayList));
            foreach (var array in arrayList)
            {
                dataRow = dataTable.Rows.Add();
                var ayylistChild = (ArrayList)Newtonsoft.Json.JsonConvert.DeserializeObject(array?.ToString(), typeof(ArrayList));
                for (int j = 0; j < ayylistChild.Count; j++)
                {
                    dataRow[j] = ayylistChild[j];
                }
            }
            await Task.CompletedTask;

            return dataTable;
        }

    }


}
