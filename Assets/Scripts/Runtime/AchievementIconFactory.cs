using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Iconos procedurales simples para logros. Mantiene una familia visual limpia
    /// sin depender de assets externos ni de referencias que puedan quedar nulas.
    /// </summary>
    public static class AchievementIconFactory
    {
        public static Sprite MakeIcon(string id, int size)
        {
            string key = (id ?? string.Empty).Trim().ToUpperInvariant();
            switch (key)
            {
                case "FIRST_GAME": return MakePerson(size);
                case "WELCOME_RHYTHM": return MakeDoor(size);
                case "FIRST_BEAT":
                case "FIRST_SONG": return MakeMusicNote(size);
                case "COMBO_50":
                case "COMBO_100":
                case "COMBO_200": return MakeChain(size, key == "COMBO_200");
                case "ACC_90":
                case "ACC_95": return MakeTarget(size);
                case "ACC_100": return MakeDiamond(size);
                case "RANK_A": return MakeRank(size, 'A');
                case "RANK_S": return MakeRank(size, 'S');
                case "FULL_COMBO": return MakeSpark(size);
                case "ALMOST_PERFECT": return MakeShield(size);
                case "MASTER_RHYTHM":
                case "MASTER_PATH": return MakeCrown(size);
                case "ACELERADA_CLEAR": return MakeLightning(size);
                case "FUNK_UNLOCKED": return MakeKey(size);
                case "SUMMER_BEAT": return MakeSun(size);
                case "ESTRELLA_RHYTHM": return MakeStar(size);
                case "FRONTLINE_READY": return MakeShield(size);
                case "REQUIEM_CLEAR": return MakeMoon(size);
                case "HER_NAME_IS_CLEAR": return MakeHeart(size);
                case "FEARLESS_FAIL": return MakeWarning(size);
                case "PERSISTENT": return MakeRepeat(size);
                case "DEDICATED": return MakeClock(size);
                case "STEADY_RHYTHM": return MakePulse(size);
                case "RANK_COLLECTOR": return MakeMedal(size);
                case "SECRET_PROJECTBEAT": return MakeQuestion(size);
                case "SECRET_RITMO": return MakeEye(size);
                case "SECRET_FEEL": return MakeHeartNote(size);
                case "SECRET_LOCKED": return MakeLock(size);
                default: return MakeTrophy(size);
            }
        }

        private static Texture2D NewTex(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(1f, 1f, 1f, 0f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++) tex.SetPixel(x, y, clear);
            return tex;
        }

        private static Sprite ToSprite(Texture2D tex, int size)
        {
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Vector2 Center(int size) => new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        private static void Draw(Texture2D tex, int x, int y, bool draw)
        {
            if (draw) tex.SetPixel(x, y, Color.white);
        }

        private static Sprite MakeTrophy(int size)
        {
            Texture2D tex = NewTex(size); Vector2 c = Center(size);
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x, y) - c;
                bool cup = p.y > -size * 0.08f && p.y < size * 0.31f && Mathf.Abs(p.x) < Mathf.Lerp(size * 0.29f, size * 0.20f, Mathf.InverseLerp(-size * 0.08f, size * 0.31f, p.y));
                bool handles = (Mathf.Abs(Mathf.Pow((p.x + size * 0.36f)/(size*0.15f),2f)+Mathf.Pow((p.y-size*0.12f)/(size*0.18f),2f)-1f)<0.22f && p.x < -size*0.18f) ||
                               (Mathf.Abs(Mathf.Pow((p.x - size * 0.36f)/(size*0.15f),2f)+Mathf.Pow((p.y-size*0.12f)/(size*0.18f),2f)-1f)<0.22f && p.x > size*0.18f);
                bool stem = Mathf.Abs(p.x) < size * 0.07f && p.y > -size * 0.34f && p.y < -size * 0.06f;
                bool baseA = Mathf.Abs(p.y + size * 0.36f) < size * 0.04f && Mathf.Abs(p.x) < size * 0.25f;
                bool baseB = Mathf.Abs(p.y + size * 0.44f) < size * 0.04f && Mathf.Abs(p.x) < size * 0.36f;
                Draw(tex, x, y, cup || handles || stem || baseA || baseB);
            }
            return ToSprite(tex, size);
        }

        private static Sprite MakePerson(int size)
        {
            Texture2D tex = NewTex(size); Vector2 c = Center(size);
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x, y) - c;
                bool ring = Mathf.Abs(p.magnitude - size * 0.40f) < size * 0.035f;
                bool head = Vector2.Distance(p, new Vector2(0f, size * 0.12f)) < size * 0.13f;
                bool body = Mathf.Abs(p.x) < size * 0.24f && p.y > -size * 0.24f && p.y < -size * 0.08f;
                Draw(tex, x, y, ring || head || body);
            }
            return ToSprite(tex, size);
        }

        private static Sprite MakeMusicNote(int size)
        {
            Texture2D tex = NewTex(size);
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float xf=x/(float)size, yf=y/(float)size;
                bool stem = xf > .57f && xf < .65f && yf > .30f && yf < .80f;
                bool flag = yf > .70f && yf < .82f && xf > .62f && xf < .84f;
                bool head = Mathf.Pow((xf-.43f)/.19f,2f)+Mathf.Pow((yf-.28f)/.12f,2f)<1f;
                Draw(tex,x,y,stem||flag||head);
            }
            return ToSprite(tex, size);
        }

        private static Sprite MakeChain(int size, bool doubleSpark)
        {
            Texture2D tex = NewTex(size); Vector2 c=Center(size);
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            {
                Vector2 p=new Vector2(x,y)-c;
                bool l1=Mathf.Abs(Mathf.Pow((p.x+size*.16f)/(size*.18f),2f)+Mathf.Pow(p.y/(size*.11f),2f)-1f)<.28f&&p.x<size*.04f;
                bool l2=Mathf.Abs(Mathf.Pow((p.x-size*.16f)/(size*.18f),2f)+Mathf.Pow(p.y/(size*.11f),2f)-1f)<.28f&&p.x>-size*.04f;
                bool mid=Mathf.Abs(p.y)<size*.035f&&Mathf.Abs(p.x)<size*.18f;
                bool spark=doubleSpark && (Mathf.Abs(p.x-size*.36f)+Mathf.Abs(p.y-size*.28f)<size*.10f);
                Draw(tex,x,y,l1||l2||mid||spark);
            }
            return ToSprite(tex,size);
        }

        private static Sprite MakeTarget(int size)
        {
            Texture2D tex=NewTex(size); Vector2 c=Center(size);
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            {
                Vector2 p=new Vector2(x,y)-c; float r=p.magnitude;
                bool rings=Mathf.Abs(r-size*.32f)<size*.025f||Mathf.Abs(r-size*.18f)<size*.025f||r<size*.055f;
                bool cross=(Mathf.Abs(p.x)<size*.018f||Mathf.Abs(p.y)<size*.018f)&&r<size*.38f;
                Draw(tex,x,y,rings||cross);
            }
            return ToSprite(tex,size);
        }

        private static Sprite MakeRank(int size,char letter)
        {
            Texture2D tex=NewTex(size); Vector2 c=Center(size);
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            {
                Vector2 p=new Vector2(x,y)-c; bool d=Mathf.Abs(p.y+size*.34f)<size*.025f&&Mathf.Abs(p.x)<size*.34f;
                if(letter=='A') { d|=Mathf.Abs(Mathf.Abs(p.x)-(p.y+size*.10f)*.55f)<size*.035f&&p.y>-size*.23f&&p.y<size*.28f; d|=Mathf.Abs(p.y-size*.02f)<size*.03f&&Mathf.Abs(p.x)<size*.16f; }
                else { float r1=Vector2.Distance(p,new Vector2(0,size*.13f)); float r2=Vector2.Distance(p,new Vector2(0,-size*.13f)); d|=(Mathf.Abs(r1-size*.20f)<size*.035f&&p.x<size*.20f)||(Mathf.Abs(r2-size*.20f)<size*.035f&&p.x>-size*.20f); d|=Mathf.Abs(p.y)<size*.03f&&Mathf.Abs(p.x)<size*.20f; }
                Draw(tex,x,y,d);
            }
            return ToSprite(tex,size);
        }

        private static Sprite MakeSpark(int size) { return MakeStar(size); }
        private static Sprite MakeStar(int size)
        {
            Texture2D tex=NewTex(size); Vector2 c=Center(size);
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            { Vector2 p=new Vector2(x,y)-c; float a=Mathf.Atan2(p.y,p.x); float r=p.magnitude; float sr=size*(.18f+.09f*Mathf.Abs(Mathf.Sin(a*5f))); Draw(tex,x,y,r<sr); }
            return ToSprite(tex,size);
        }

        private static Sprite MakeCrown(int size)
        {
            Texture2D tex=NewTex(size); Vector2 c=Center(size);
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            { Vector2 p=new Vector2(x,y)-c; bool crown=p.y>-size*.06f&&p.y<size*.25f&&Mathf.Abs(p.x)<size*.36f&&p.y<size*.02f+Mathf.Abs(Mathf.Sin((p.x/size+.5f)*Mathf.PI*3f))*size*.22f; bool baseL=Mathf.Abs(p.y+size*.08f)<size*.04f&&Mathf.Abs(p.x)<size*.36f; Draw(tex,x,y,crown||baseL); }
            return ToSprite(tex,size);
        }

        private static Sprite MakeLightning(int size)
        {
            Texture2D tex=NewTex(size); Vector2 c=Center(size);
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            { Vector2 p=new Vector2(x,y)-c; bool top=p.y>0&&p.y<size*.35f&&p.x>-.12f*size&&p.x<.18f*size-(p.y/size)*.4f*size; bool bot=p.y<0&&p.y>-size*.36f&&p.x<.13f*size&&p.x>-.20f*size-(p.y/size)*.4f*size; Draw(tex,x,y,top||bot); }
            return ToSprite(tex,size);
        }

        private static Sprite MakeKey(int size)
        {
            Texture2D tex=NewTex(size); Vector2 c=Center(size);
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            { Vector2 p=new Vector2(x,y)-c; bool ring=Mathf.Abs(Vector2.Distance(p,new Vector2(-size*.18f,size*.08f))-size*.13f)<size*.035f; bool shaft=Mathf.Abs(p.y-size*.02f)<size*.035f&&p.x>-size*.06f&&p.x<size*.34f; bool teeth=p.x>size*.18f&&p.x<size*.28f&&p.y>-size*.13f&&p.y<size*.02f; Draw(tex,x,y,ring||shaft||teeth); }
            return ToSprite(tex,size);
        }

        private static Sprite MakeSun(int size)
        {
            Texture2D tex=NewTex(size); Vector2 c=Center(size);
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            { Vector2 p=new Vector2(x,y)-c; float r=p.magnitude; float a=Mathf.Atan2(p.y,p.x); bool core=r<size*.19f; bool rays=r>size*.26f&&r<size*.38f&&Mathf.Abs(Mathf.Sin(a*8f))<.18f; Draw(tex,x,y,core||rays); }
            return ToSprite(tex,size);
        }

        private static Sprite MakeShield(int size)
        {
            Texture2D tex=NewTex(size); Vector2 c=Center(size);
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            { Vector2 p=new Vector2(x,y)-c; bool d=p.y<size*.30f&&p.y>-size*.38f&&Mathf.Abs(p.x)<Mathf.Lerp(size*.08f,size*.30f,Mathf.InverseLerp(-size*.38f,size*.30f,p.y)); bool top=Mathf.Abs(p.y-size*.30f)<size*.035f&&Mathf.Abs(p.x)<size*.30f; Draw(tex,x,y,d||top); }
            return ToSprite(tex,size);
        }

        private static Sprite MakeMoon(int size)
        {
            Texture2D tex=NewTex(size); Vector2 c=Center(size);
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            { Vector2 p=new Vector2(x,y)-c; bool a=p.magnitude<size*.32f; bool b=Vector2.Distance(p,new Vector2(size*.13f,size*.06f))<size*.31f; Draw(tex,x,y,a&&!b); }
            return ToSprite(tex,size);
        }

        private static Sprite MakeHeart(int size)
        {
            Texture2D tex=NewTex(size); Vector2 c=Center(size);
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            { Vector2 p=(new Vector2(x,y)-c)/(size*.24f); float xx=p.x, yy=p.y; bool d=Mathf.Pow(xx*xx+yy*yy-1f,3f)-xx*xx*yy*yy*yy<0f && yy>-1.25f; Draw(tex,x,y,d); }
            return ToSprite(tex,size);
        }

        private static Sprite MakeDiamond(int size)
        { Texture2D tex=NewTex(size); Vector2 c=Center(size); for(int y=0;y<size;y++) for(int x=0;x<size;x++){Vector2 p=new Vector2(x,y)-c; Draw(tex,x,y,Mathf.Abs(p.x)+Mathf.Abs(p.y)<size*.32f);} return ToSprite(tex,size); }
        private static Sprite MakeWarning(int size)
        { Texture2D tex=NewTex(size); Vector2 c=Center(size); for(int y=0;y<size;y++) for(int x=0;x<size;x++){Vector2 p=new Vector2(x,y)-c; bool tri=p.y>-size*.30f&&p.y<size*.30f&&Mathf.Abs(p.x)<(size*.32f-p.y*.45f); bool cut=Mathf.Abs(p.x)<size*.025f&&p.y>-size*.13f&&p.y<size*.13f; Draw(tex,x,y,(tri&&Mathf.Abs(p.x)>size*.03f)||cut);} return ToSprite(tex,size); }
        private static Sprite MakeRepeat(int size)
        { Texture2D tex=NewTex(size); Vector2 c=Center(size); for(int y=0;y<size;y++) for(int x=0;x<size;x++){Vector2 p=new Vector2(x,y)-c; float r=p.magnitude; bool arc=Mathf.Abs(r-size*.28f)<size*.025f&&!(p.x>size*.16f&&p.y<-size*.16f); bool arrow=p.x>size*.18f&&p.y>size*.12f&&Mathf.Abs(p.x-p.y)<size*.10f; Draw(tex,x,y,arc||arrow);} return ToSprite(tex,size); }
        private static Sprite MakeClock(int size)
        { Texture2D tex=NewTex(size); Vector2 c=Center(size); for(int y=0;y<size;y++) for(int x=0;x<size;x++){Vector2 p=new Vector2(x,y)-c; bool ring=Mathf.Abs(p.magnitude-size*.31f)<size*.025f; bool hand=(Mathf.Abs(p.x)<size*.022f&&p.y>0&&p.y<size*.18f)||(Mathf.Abs(p.y)<size*.022f&&p.x>0&&p.x<size*.15f); Draw(tex,x,y,ring||hand);} return ToSprite(tex,size); }
        private static Sprite MakePulse(int size)
        { Texture2D tex=NewTex(size); for(int y=0;y<size;y++) for(int x=0;x<size;x++){float xf=x/(float)size, yf=y/(float)size; float line=.5f+.12f*Mathf.Sin(xf*20f); Draw(tex,x,y,Mathf.Abs(yf-line)<.025f);} return ToSprite(tex,size); }
        private static Sprite MakeMedal(int size)
        { Texture2D tex=NewTex(size); Vector2 c=Center(size); for(int y=0;y<size;y++) for(int x=0;x<size;x++){Vector2 p=new Vector2(x,y)-c; bool ribbon=p.y>size*.10f&&p.y<size*.38f&&Mathf.Abs(p.x)<size*.16f; bool coin=Vector2.Distance(p,new Vector2(0,-size*.12f))<size*.22f; Draw(tex,x,y,ribbon||coin);} return ToSprite(tex,size); }
        private static Sprite MakeQuestion(int size)
        { Texture2D tex=NewTex(size); Vector2 c=Center(size); for(int y=0;y<size;y++) for(int x=0;x<size;x++){Vector2 p=new Vector2(x,y)-c; float r=Vector2.Distance(p,new Vector2(0,size*.12f)); bool top=Mathf.Abs(r-size*.18f)<size*.035f&&p.y>0; bool mid=Mathf.Abs(p.x)<size*.035f&&p.y>-size*.13f&&p.y<size*.08f; bool dot=Vector2.Distance(p,new Vector2(0,-size*.28f))<size*.045f; Draw(tex,x,y,top||mid||dot);} return ToSprite(tex,size); }
        private static Sprite MakeEye(int size)
        { Texture2D tex=NewTex(size); Vector2 c=Center(size); for(int y=0;y<size;y++) for(int x=0;x<size;x++){Vector2 p=new Vector2(x,y)-c; bool eye=Mathf.Abs(Mathf.Pow(p.x/(size*.36f),2f)+Mathf.Pow(p.y/(size*.16f),2f)-1f)<.18f; bool pupil=p.magnitude<size*.08f; Draw(tex,x,y,eye||pupil);} return ToSprite(tex,size); }
        private static Sprite MakeHeartNote(int size)
        { return MakeHeart(size); }
        private static Sprite MakeLock(int size)
        { Texture2D tex=NewTex(size); Vector2 c=Center(size); for(int y=0;y<size;y++) for(int x=0;x<size;x++){Vector2 p=new Vector2(x,y)-c; bool body=Mathf.Abs(p.x)<size*.24f&&p.y>-size*.22f&&p.y<size*.10f; bool shackle=Mathf.Abs(Mathf.Pow(p.x/(size*.18f),2f)+Mathf.Pow((p.y-size*.12f)/(size*.20f),2f)-1f)<.22f&&p.y>size*.07f; Draw(tex,x,y,body||shackle);} return ToSprite(tex,size); }
        private static Sprite MakeDoor(int size)
        { Texture2D tex=NewTex(size); Vector2 c=Center(size); for(int y=0;y<size;y++) for(int x=0;x<size;x++){Vector2 p=new Vector2(x,y)-c; bool frame=Mathf.Abs(p.x)<size*.25f&&Mathf.Abs(p.y)<size*.35f&&(!(Mathf.Abs(p.x)<size*.19f&&Mathf.Abs(p.y)<size*.28f)); bool knob=Vector2.Distance(p,new Vector2(size*.10f,-size*.02f))<size*.035f; Draw(tex,x,y,frame||knob);} return ToSprite(tex,size); }
    }
}
