using MaxstUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Maxst.Avatar
{
    public enum DIComponent
    {
        none,
        AvatarView,
    }

    public class DI : DIBase
    {
        public DI(DIScope scope, DIComponent component = DIComponent.none) : base()
        {
            Scope = scope;
            ScopeName = component.ToString();
        }
    }

    public class ProjectSceneScope : SceneScope<DIComponent>
    {

    }
}