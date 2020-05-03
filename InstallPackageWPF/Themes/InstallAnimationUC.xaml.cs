using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InstallPackageWPF.Themes
{
    /// <summary>
    /// InstallAnimationUC.xaml 的交互逻辑
    /// </summary>
    public partial class InstallAnimationUC : UserControl
    {
        System.Windows.Threading.DispatcherTimer frameTimer;

        private Point3D spawnPoint;
        private double elapsed;
        private double totalElapsed;


        private ParticleSystemManager pm;

        private Random rand;
        public InstallAnimationUC()
        {
            InitializeComponent();
            frameTimer = new System.Windows.Threading.DispatcherTimer();
            frameTimer.Tick += OnFrame;
            frameTimer.Interval = TimeSpan.FromSeconds(1.0 / 60.0);
            frameTimer.Start();

            this.spawnPoint = new Point3D(0.0, 0.0, 0.0);

            pm = new ParticleSystemManager();

            this.WorldModels.Children.Add(pm.CreateParticleSystem(500, (Color)ColorConverter.ConvertFromString("#FF9AF7FE")));

            rand = new Random(this.GetHashCode());

        }
        private void OnFrame(object sender, EventArgs e)
        {
            this.elapsed = 0.05;
            this.totalElapsed += this.elapsed;

            pm.Update((float)elapsed);

            pm.SpawnParticle(this.spawnPoint, 10.0, (Color)ColorConverter.ConvertFromString("#FF9AF7FE"), rand.NextDouble(), rand.NextDouble());
            pm.SpawnParticle(this.spawnPoint, 10.0, (Color)ColorConverter.ConvertFromString("#FF9AF7FE"), 1.3 * rand.NextDouble(), rand.NextDouble());
            pm.SpawnParticle(this.spawnPoint, 5.0, (Color)ColorConverter.ConvertFromString("#FF9AF7FE"), 1.2 * rand.NextDouble(), rand.NextDouble());
            pm.SpawnParticle(this.spawnPoint, 5.0, (Color)ColorConverter.ConvertFromString("#FF9AF7FE"), rand.NextDouble(), rand.NextDouble());
            pm.SpawnParticle(this.spawnPoint, 15.0, (Color)ColorConverter.ConvertFromString("#FF9AF7FE"), 2 * rand.NextDouble(), rand.NextDouble());
            pm.SpawnParticle(this.spawnPoint, 15.0, (Color)ColorConverter.ConvertFromString("#FF9AF7FE"), 3 * rand.NextDouble(), rand.NextDouble());
            pm.SpawnParticle(this.spawnPoint, 15.0, (Color)ColorConverter.ConvertFromString("#FF9AF7FE"), 2 * rand.NextDouble(), rand.NextDouble());
            pm.SpawnParticle(this.spawnPoint, 5.0, (Color)ColorConverter.ConvertFromString("#FF9AF7FE"), rand.NextDouble(), rand.NextDouble());
            pm.SpawnParticle(this.spawnPoint, 10.0, (Color)ColorConverter.ConvertFromString("#FF9AF7FE"), 1.5 * rand.NextDouble(), rand.NextDouble());
            pm.SpawnParticle(this.spawnPoint, 10.0, (Color)ColorConverter.ConvertFromString("#FF9AF7FE"), rand.NextDouble(), rand.NextDouble());
            pm.SpawnParticle(this.spawnPoint, 5.0, (Color)ColorConverter.ConvertFromString("#FF9AF7FE"), 1.2 * rand.NextDouble(), rand.NextDouble());
            pm.SpawnParticle(this.spawnPoint, 5.0, (Color)ColorConverter.ConvertFromString("#FF9AF7FE"), rand.NextDouble(), rand.NextDouble());

            double x = Math.Cos(this.totalElapsed);
            double y = Math.Sin(this.totalElapsed);
            this.spawnPoint = new Point3D(x * 32.0, y * 11.0, 0.0);
        }
    }
}
