using System.Globalization;
using System.Text.RegularExpressions;

namespace WeChat_AccInfo.dat
{
    public class UserInfo
    {
        private string id;
        private string name;
        private string[] email;
        private string[] phone;
        private string[] avatar;
        private string[] other;

        public UserInfo(string id, string name, string[] email, string[] phone, string[] avatar, string[] other)
        {
            this.id = id;
            this.name = name;
            this.email = email;
            this.phone = phone;
            this.avatar = avatar;
            this.other = other;
        }

        public override string ToString()
        {
            string result = "";
            result += $"Id:\t{id}\n";
            result += $"Name:\t{name}\n";
            result += $"Email:\t[{String.Join(", ", email)}]\n";
            result += $"Phone:\t[{String.Join(", ", phone)}]\n";
            result += $"Avatar:\t[{String.Join(", ", avatar)}]\n";
            result += $"Other:\t[{String.Join(", ", other)}]";
            return result;
        }
    }

    public class WeChatUserInfo(string rootPath)
    {
        /// <summary>
        /// 获取用户路径列表，该列表包含具有特定配置文件的目录路径。
        /// </summary>
        /// <returns>包含 AccInfo.dat 配置文件的目录路径数组</returns>
        private string[] GetUserPaths()
        {
            List<string> userPaths = new List<string>();

            // 遍历根目录下的所有子目录
            try
            {
                foreach (var directory in Directory.GetDirectories(rootPath))
                {
                    // 构建 AccInfo.dat 的完整路径
                    string path = $@"{directory}\config\AccInfo.dat";

                    // 如果该路径下存在 AccInfo.dat 文件，则将路径添加到列表中
                    if (File.Exists(path))
                    {
                        userPaths.Add(path);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: Please enter the correct folder directory");
            }

            // 将列表转换为数组并返回
            return userPaths.ToArray();
        }

        /// <summary>
        /// 获取用户信息列表，从包含特定格式数据的文件中提取用户信息并返回。
        /// </summary>
        /// <returns>包含用户信息的 UserInfo 对象列表</returns>
        public List<UserInfo> GetUserInfos()
        {
            List<UserInfo> userInfos = new List<UserInfo>();

            // 遍历所有用户路径
            foreach (var path in GetUserPaths())
            {
                try
                {
                    // 读取文件内容
                    string fileTexts = File.ReadAllText(path);

                    // 提取微信 ID
                    string weChatId =
                        ReplaceInvisibleCharacters(QueryFileInfo(fileTexts, "\b\u0004\u0012", "\b\n\u0012"));

                    // 提取其他未解析的数据
                    string otherData = QueryFileInfo(fileTexts, "\b\n\u0012");

                    // 提取微信名称和其他信息
                    List<Match> enumerable = ExtractInvisibleCharacters(otherData)
                        .Matches(otherData).ToList()
                        .Where(match => !string.IsNullOrWhiteSpace(match.Value.Trim())).ToList();

                    string weChatName = enumerable[0].Value.Trim();

                    // 初始化列表用于存储不同类型的信息
                    List<string> weChatPhone = new List<string>();
                    List<string> weChatEmail = new List<string>();
                    List<string> weChatOther = new List<string>();
                    List<string> weChatAvatar = new List<string>();

                    // 正则表达式用于匹配邮箱、手机号和链接
                    Regex emailRegex =
                        new Regex(
                            @"[\w!#$%&'*+/=?^_`{|}~-]+(?:\.[\w!#$%&'*+/=?^_`{|}~-]+)*@(?:[\w](?:[\w-]*[\w])?\.)+[\w](?:[\w-]*[\w])?");
                    Regex phoneRegex = new Regex(@"^1\d{10}$");
                    Regex urlRegex = new Regex(@"^https?://([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?");

                    // 遍历其他信息并根据类型进行分类
                    foreach (var match in enumerable.GetRange(1, enumerable.Count - 1))
                    {
                        string text = match.Value.Trim();

                        if (emailRegex.IsMatch(text))
                        {
                            weChatEmail.Add(text);
                            continue;
                        }

                        if (phoneRegex.IsMatch(text))
                        {
                            weChatPhone.Add(text);
                            continue;
                        }

                        if (urlRegex.IsMatch(text))
                        {
                            weChatAvatar.Add(text);
                            continue;
                        }

                        weChatOther.Add(text);
                    }

                    // 创建 UserInfo 对象并添加到列表中
                    userInfos.Add(new UserInfo(weChatId, weChatName, weChatEmail.ToArray(), weChatPhone.ToArray(),
                        weChatAvatar.ToArray(), weChatOther.ToArray()));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("File read failed: " + ex.Message);
                }
            }

            return userInfos;
        }

        /// <summary>
        /// 从给定的文本中查询信息，返回包含搜索字符串和结束字符串之间的内容。
        /// </summary>
        /// <param name="text">要查询的文本</param>
        /// <param name="searchString">开始搜索的字符串</param>
        /// <param name="endString">结束搜索的字符串（可选）</param>
        /// <returns>搜索字符串和结束字符串之间的内容，如果找不到则返回空字符串</returns>
        private string QueryFileInfo(string text, string searchString, string endString = "")
        {
            if (!String.IsNullOrEmpty(searchString) && !String.IsNullOrEmpty(endString))
            {
                // 查找起始字符串的索引
                int startIndex = text.IndexOf(searchString, StringComparison.Ordinal);

                // 查找结束字符串的索引
                int endIndex = text.IndexOf(endString, startIndex + searchString.Length, StringComparison.Ordinal);

                if (startIndex != -1 && endIndex != -1)
                {
                    // 返回包含搜索字符串和结束字符串之间的内容
                    return text.Substring(startIndex, endIndex + endString.Length - startIndex);
                }
            }
            else if (!String.IsNullOrEmpty(searchString) && String.IsNullOrEmpty(endString))
            {
                // 查找起始字符串的索引
                int startIndex = text.IndexOf(searchString, StringComparison.Ordinal);

                if (startIndex != -1)
                {
                    // 返回从搜索字符串到文本末尾的内容
                    return text.Substring(startIndex);
                }
            }

            // 如果未找到匹配的内容，则返回空字符串
            return "";
        }

        /// <summary>
        /// 从输入字符串中提取不可见字符，并返回一个正则表达式，用于匹配包含不可见字符的内容。
        /// 不可见字符包括空格、制表符、换行符等。
        /// </summary>
        /// <param name="input">要处理的输入字符串</param>
        /// <returns>用于匹配包含不可见字符的内容的正则表达式</returns>
        private Regex ExtractInvisibleCharacters(string input)
        {
            string result = "";

            // 遍历输入字符串，提取不可见字符
            foreach (char c in input)
            {
                if (IsInvisibleCharacter(c))
                {
                    if (char.IsControl(c) && c != '\t' && c != '\n' && c != '\r')
                    {
                        // 对于控制字符，转义成Unicode编码
                        result += $"\\u{(int)c:X4}";
                    }
                    else
                    {
                        // 对于其他不可见字符，使用Regex.Escape进行转义
                        result += Regex.Escape(c.ToString());
                    }

                    // 将不可见字符添加到结果中
                    result += c;
                }
            }

            // 构造一个正则表达式，用于匹配不包含不可见字符的内容
            return new Regex($"[^{result}]*");
        }

        /// <summary>
        /// 判断字符是否为不可见字符
        /// </summary>
        /// <param name="c">要判断的字符</param>
        /// <returns>如果字符为不可见字符，则返回 true；否则返回 false</returns>
        private bool IsInvisibleCharacter(char c)
        {
            // 使用 CharUnicodeInfo.GetUnicodeCategory 方法获取字符的 Unicode 分类
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);

            // 如果字符的 Unicode 分类为 Control 或 OtherNotAssigned，则认为是不可见字符
            return category == UnicodeCategory.Control || category == UnicodeCategory.OtherNotAssigned;
        }

        /// <summary>
        /// 从输入字符串中删除不可见字符，并返回处理后的字符串。
        /// 不可见字符包括空格、制表符、换行符等。
        /// </summary>
        /// <param name="input">要处理的输入字符串</param>
        /// <returns>删除不可见字符后的字符串</returns>
        private string ReplaceInvisibleCharacters(string input)
        {
            string result = "";

            foreach (char c in input)
            {
                if (IsInvisibleCharacter(c))
                {
                    continue;
                }

                result += c;
            }

            return result;
        }
    }
}