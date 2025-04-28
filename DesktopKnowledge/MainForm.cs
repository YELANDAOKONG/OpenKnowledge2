using LibraryOpenKnowledge.Models;
using LibraryOpenKnowledge.Tools;

namespace DesktopKnowledge;

public partial class MainForm : Form
{
    
    private SystemConfig config;
    
    public MainForm()
    {
        InitializeComponent();
        config = ConfigTools.GetOrCreateSystemConfig();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        if (
            config.OpenAiApiUrl == null ||
            config.OpenAiApiKey == null ||
            config.OpenAiModel == null
        )
        {
            // 配置文件校验
            MessageBox.Show(
                "没有检测到有效的人工智能接口配置文件，请先修改配置文件！",
                "Open Knowledge Launcher",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            this.Close();
            Program.Exit(-1, true);
            return;
        }
    }
}