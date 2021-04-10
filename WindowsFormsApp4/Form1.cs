using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Server.Attributes;
using Grapevine.Shared;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace WindowsFormsApp4
{
    public partial class Form1 : Form
    {

        private RestServer server;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// [TextBoxに数字しか入力できないようにする - .NET Tips (VB.NET,C#...)](https://dobon.net/vb/dotnet/control/numerictextbox.html)
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            //数字と空白しか入力できないようにする
            this.maskedTextBox1.Mask = "9999";
            //Int32型に変換できるか検証する
            this.maskedTextBox1.ValidatingType = typeof(int);
            //TypeValidationCompletedイベントハンドラを追加する
            this.maskedTextBox1.TypeValidationCompleted +=
                maskedTextBox1_TypeValidationCompleted;
        }

        private void maskedTextBox1_TypeValidationCompleted(
            object sender, TypeValidationEventArgs e)
        {
            //Int32型に変換できるか確かめる
            if (!e.IsValidInput)
            {
                //Int32型への変換に失敗した時は、フォーカスが移動しないようにする
                MessageBox.Show("数値を入力してください");
                e.Cancel = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            ServerSettings settings = new ServerSettings()
            {
                Port = maskedTextBox1.Text,
                PublicFolder = new PublicFolder("web")
            };

            server = new RestServer(settings);
            server.Start();

            button1.Enabled = false;
            button2.Enabled = true;

        }


        private void button2_Click(object sender, EventArgs e)
        {
            server.Stop();
            button1.Enabled = true;
            button2.Enabled = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (server != null)
            {
                server.Stop();
            }
        }
    }

    /// <summary>
    /// [Route Not Found For GET / - Can't serve default index.html · Issue #209 · scottoffen/grapevine-legacy](https://github.com/scottoffen/grapevine-legacy/issues/209)
    /// </summary>
    [RestResource]
    public class TestResource
    {

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/")]
        public IHttpContext Root(IHttpContext context)
        {
            var filepath = Path.Combine(context.Server.PublicFolder.FolderPath,
                                        context.Server.PublicFolder.IndexFileName);

            var lastModified = File.GetLastWriteTimeUtc(filepath).ToString("R");
            context.Response.AddHeader("Last-Modified", lastModified);

            if (context.Request.Headers.AllKeys.Contains("If-Modified-Since"))
            {
                if (context.Request.Headers["If-Modified-Since"].Equals(lastModified))
                {
                    context.Response.SendResponse(HttpStatusCode.NotModified);
                    return context;
                }
            }

            context.Response.ContentType = ContentType.DEFAULT.FromExtension(filepath);
            context.Response.SendResponse(new FileStream(filepath, FileMode.Open));

            return context;
        }
    }

}
