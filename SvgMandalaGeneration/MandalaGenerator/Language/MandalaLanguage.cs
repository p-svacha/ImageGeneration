using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class MandalaLanguage
{
    public static List<MandalaLanguageRule> MandalaLanguageRules = new List<MandalaLanguageRule>()
    {
        new MRule_Empty_Circle(),
        new MRule_Circle_InnerCircle(),
        new MRule_Ring_Stripes(),
        new MRule_Ring_Circles(),
        new MRule_Circle_Star(),
        new MRule_Star_InnerStar(),
        new MRule_Star_InnerCircle(),
        new MRule_CircleSector_Circle(),
        new MRule_CircleSector_Stripes(),
        new MRule_StarSpike_Split(),

        
        //new MRule_CircleSector_Fill(),
        //new MRule_Stripe_Fill(),
        //new MRule_StarSpike_Fill(),
        
    };
}
