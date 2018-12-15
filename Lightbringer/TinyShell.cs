using UnityEngine;

namespace Lightbringer {
    public class TinyShell
    {
        internal static void FaceLeft(On.HeroController.orig_FaceLeft orig, HeroController self)
        {
            self.cState.facingRight = false;
            Vector3 localScale = self.transform.localScale;
            localScale.x = self.playerData.equippedCharm_4 ? 0.75f : 1f;
            self.transform.localScale = localScale;
        }

        internal static void FaceRight(On.HeroController.orig_FaceRight orig, HeroController self)
        {
            self.cState.facingRight = true;
            Vector3 localScale = self.transform.localScale;
            localScale.x = self.playerData.equippedCharm_4 ? -0.75f : -1f;
            self.transform.localScale = localScale;
        }
    }
}