using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeleteExpiredFiles.Properties;
using log4net;

namespace DeleteExpiredFiles
{
    class Program
    {
        static ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {

            logger.Debug("DeleteExpiredFiles Start");
            try
            {
                //ファイルからパスと期限がセットになったものを取得
                var list = File.ReadAllLines(Settings.Default.ConfigPath).ToList<string>();

                var today = DateTime.Now;
                var deleteFileList = new List<string>();
                var errorList = new List<Exception>();
                foreach (string line in list)
                {
                    if (string.IsNullOrEmpty(line)) continue;

                    //一行の中から削除期限とチェック対象ディレクトリを取得。[期限(day),パターン,パス]の形式で格納されている
                    if (line.Split(',').Length != 3) throw new Exception("取得したレコードの中がおかしいです" + line);

                    var limit = line.Split(',')[0];
                    var pattern = line.Split(',')[1];
                    var path = line.Split(',')[2];

                    foreach (string filePath in Directory.GetFiles(path, pattern))
                    {
                        //期限判定
                        if (today.AddDays(-int.Parse(limit)) >= File.GetLastWriteTime(filePath))
                        {
                            //削除と同時にレスポンス用にリストに格納
                            logger.Debug("削除ファイル：" + filePath);
                            try
                            {
                                File.Delete(filePath);
                                deleteFileList.Add(filePath);
                            }
                            catch (Exception e)
                            {
                                logger.Error("削除エラー：" + filePath, e);
                                //削除できなくても処理は継続する
                                errorList.Add(e);
                            }
                        }
                    }
                }

                if (deleteFileList.Count != 0 || errorList.Count != 0)
                {
                    var message = new StringBuilder();
                    if (deleteFileList.Count != 0)
                    {
                        message.Append("以下のファイルを削除いたしました。" + "\r\n");
                        foreach (string f in deleteFileList) { message.Append(f + "\r\n"); }
                    }
                    else
                    {
                        message.Append("今回、削除対象ファイルはありませんでした。" + "\r\n");
                    }

                    if (errorList.Count != 0)
                    {
                        if (!string.IsNullOrEmpty(message.ToString())) { message.Append("\r\n"); }

                        message.Append("削除処理にて以下のエラーが発生しました" + "\r\n");
                        foreach (Exception f in errorList) { message.Append(f.Message + "\r\n" + "\r\n"); }
                    }

                    //メール送信
                    Mail.SendMail("期限切れファイル削除処理 削除ファイル通知", message.ToString());
                }

            }
            catch (Exception ex)
            {
                logger.Error("処理エラー", ex);

                //メール送信
                var message = "ファイル削除処理にてエラーが発生しました。";
                message += "\r\n" + ex.Message;
                message += "\r\n" + ex.StackTrace;
                try
                {
                    Mail.SendMail("期限切れファイル削除処理 Error", message);
                }
                catch (Exception mailEx) { logger.Error("メール送信エラー", mailEx); }
            }
            finally
            {
                logger.Debug("DeleteExpiredFiles End");
            }
        }

        /// <summary> メールで送信した際にどこのどういう処理がエラーになったのかがわかるようにする </summary>
        public static string MakeBatchMessage()
        {
            string message = "";
            message += "----------------------------------------------" + "\r\n";
            message += "処理名：" + "保持期間切れのファイルを削除(DeleteExpiredFiles)" + "\r\n";
            if (Settings.Default.DevFlg) message += "<<検証環境>>\r\n";
            message += "処理概要：" + "\r\n" +
                        "ファイルにて指定された保持期限が切れたファイルを削除する" + "\r\n";
            message += "実行サーバー：" + Settings.Default.RunServer + "\r\n";
            message += "処理起動場所：" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\r\n";
            message += "----------------------------------------------" + "\r\n";

            return message;
        }

    }
}
