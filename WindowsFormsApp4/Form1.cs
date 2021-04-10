using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Server.Attributes;
using Grapevine.Shared;
using System.IO;


namespace WindowsFormsApp4
{
    public partial class Form1 : Form
    {

        private RestServer myServer;

        public Form1()
        {
            InitializeComponent();
        }


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
                //Host = "*",
                //Port = "8080",
                Port = maskedTextBox1.Text,
                PublicFolder = new PublicFolder("Web")
            };

            myServer = new RestServer(settings);
            myServer.Start();

            button1.Enabled = false;
            button2.Enabled = true;

            //////SpendLongTimeメソッドを実行するための
            //////Threadオブジェクトを作成する
            //System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(SpendLongTime));
            //////スレッドを開始する
            //t.Start();

            //ServerSettings settings = new ServerSettings()
            //{
            //    //Host = "*",
            //    Port = "8080",
            //    PublicFolder = new PublicFolder("Web")
            //};

            //using (var server = new RestServer(settings))
            ////using (var server = new RestServer())
            //{
            //    //server.LogToConsole();

            //    //server.OnBeforeStart = () => server.Logger.Trace("Starting Server");
            //    //server.OnAfterStart = () => server.Logger.Trace("Server Started");
            //    //server.OnBeforeStop = () => server.Logger.Trace("Stopping Server");
            //    //server.OnAfterStop = () => server.Logger.Trace("Server Stopped");
            //    //server.Router.BeforeRouting += ctx => server.Logger.Debug("Before Routing!!");
            //    //server.Router.BeforeRouting += ctx => server.Logger.Debug("After Routing!!");

            //    server.Start();
            //    //Console.ReadLine();
            //    //server.Stop();

            //    //while (true)
            //    //{
            //    //    ;
            //    //}
            //}
        }

        //private void SpendLongTime()
        //{
        //    ServerSettings settings = new ServerSettings()
        //    {
        //        //Host = "*",
        //        Port = "8080",
        //        PublicFolder = new PublicFolder("Web")
        //    };

        //    using (var server = new RestServer(settings))
        //    //using (var server = new RestServer())
        //    {
        //        server.Start();
        //        while (true)
        //        {
        //            ;
        //        }
        //    }

        //}

        private void button2_Click(object sender, EventArgs e)
        {
            myServer.Stop();
            button1.Enabled = true;
            button2.Enabled = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (myServer != null)
            {
                myServer.Stop();
            }
        }
    }

    [RestResource]
    public class TestResource
    {
        //[RestRoute(HttpMethod = HttpMethod.ALL, PathInfo = "^.*$")]
        //public IHttpContext LevelOne(IHttpContext context)
        //{
        //    // throw new Exception("Killing It!");
        //    return context;
        //}


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
