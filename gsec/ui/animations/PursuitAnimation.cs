using gsec.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.ui.animations
{
    public class PursuitAnimation : BaseAnimation
    {
        Ranger ranger;

        protected override double DurationSeconds => 5;

        public PursuitAnimation(Ranger ranger, Action<BaseAnimation> onFinish = null)
        {
            this.ranger = ranger;
            OnFinish = onFinish;
        }

        protected override void Update(double elapsedSeconds)
        {
            throw new NotImplementedException();
        }
    }
}
