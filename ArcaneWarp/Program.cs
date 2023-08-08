using SDL2;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using static SDL2.SDL;
using static SDL2.SDL_image;

namespace AracaneWarp
{
    class Program
    {

        struct Velocity
        {
            public float x;
            public float y;
        }

        float velocityGoal;

        int WINDOW_WIDTH = 700;
        int WINDOW_HEIGHT = 500;

        public int targetFps = 60;
        float gravity = 9.8f;

        nint window;
        public nint renderer;

        //Structs
        SDL_Rect playerRect;
        SDL_Rect playerSrect;
        SDL_Rect colliosion;
        //Velocity playerVelocity;
        Velocity playerVelocityGoal;

        SDL_Rect[] Objects = new SDL_Rect[]
        {
            new SDL_Rect{x = 0,y = 370 ,w = 700,h = 30},
            new SDL_Rect{x = 300,y = 200 ,w = 350,h = 30},
        };

        //Textures
        nint playerTextureIdle; //Not used
        nint playerTextureRun; //Not used
        nint backgroundtexture;

        //Temp textures
        nint temp_floorTexture;
        //Init
        int PLAYER_WIDTH = 0;
        int PLAYER_HEIGHT = 0;
        Vector2 playerVelocity = new Vector2(0, 0);
        EntitySystem player = new EntitySystem();

        int TEXTURE_WIDTH;
        int TEXTURE_HEIGHT;

        public float deltaTime;
        float buferJump;

        bool jumpS = false;
        bool run = false;

        public Program()
        {
            SDL_Init(SDL_INIT_VIDEO);

            SDL_CreateWindowAndRenderer(WINDOW_WIDTH, WINDOW_HEIGHT, 0, out window, out renderer);

            backgroundtexture = SDL_CreateTextureFromSurface(renderer, SDL_image.IMG_Load("Background.png"));
            playerTextureIdle = SDL_CreateTextureFromSurface(renderer, SDL_image.IMG_Load("Character/Idle/Idle-Sheet.png"));
            playerTextureRun = SDL_CreateTextureFromSurface(renderer, SDL_image.IMG_Load("Character/Run/Run-Sheet.png"));
            temp_floorTexture = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_RGBA8888, (int)SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, 600, 30);
            nint playerSurface = SDL_image.IMG_Load("Character/Idle/Idle-Sheet.png");
            player.createTexture(renderer, playerSurface);
            SDL_QueryTexture(playerTextureRun, out _, out _, out TEXTURE_WIDTH, out TEXTURE_HEIGHT);
            PLAYER_WIDTH = TEXTURE_WIDTH / 8;
            PLAYER_HEIGHT = TEXTURE_HEIGHT;

            SDL_QueryTexture(playerTextureIdle, out _, out _, out TEXTURE_WIDTH, out TEXTURE_HEIGHT);
            PLAYER_WIDTH = TEXTURE_WIDTH / 4;
            PLAYER_HEIGHT = TEXTURE_HEIGHT;

            playerRect.x = 5;
            playerRect.y = 5;
            playerRect.w = PLAYER_WIDTH;
            playerRect.h = PLAYER_HEIGHT;

            playerSrect.x = 0;
            playerSrect.y = 0;
            playerSrect.w = PLAYER_WIDTH;
            playerSrect.h = PLAYER_HEIGHT;

            playerVelocityGoal.y = 0;

        }
        ~Program()
        {
            SDL_DestroyWindow(window);
            SDL_DestroyRenderer(renderer);
            SDL_DestroyTexture(backgroundtexture);
            SDL_DestroyTexture(playerTextureIdle);
            SDL_DestroyTexture(playerTextureRun);
            SDL_Quit();
        }

        public static int Main()
        {
            Program prag = new Program();

            SDL_Event events;

            bool loop = true;

            const int FPS = 30;
            int frameTime = 0;



            UInt32 lasTime = SDL_GetTicks();
            while (loop)
            {
                UInt32 nowTime = SDL_GetTicks();
                prag.deltaTime = (nowTime - lasTime);
                //if the delta time is higher than 0.15f for every frame the speed will be to high
                if (prag.deltaTime > 0.15f)
                {
                    prag.deltaTime = 0.15f;
                }
                if (prag.run == false)
                {
                    SDL_QueryTexture(prag.playerTextureIdle, out _, out _, out prag.TEXTURE_WIDTH, out prag.TEXTURE_HEIGHT);
                    prag.PLAYER_WIDTH = prag.TEXTURE_WIDTH / 4;
                    prag.PLAYER_HEIGHT = prag.TEXTURE_HEIGHT;
                    Console.WriteLine(prag.playerRect.x);
                }

                while (SDL_PollEvent(out events) == 1)
                {
                    switch (events.type)
                    {
                        case SDL_EventType.SDL_QUIT:
                            loop = false;
                            break;
                        case SDL_EventType.SDL_KEYDOWN:

                            if (prag.GetKey(SDL_Keycode.SDLK_LEFT))
                            {
                                prag.playerVelocityGoal.x = -15f;
                            }
                            else if (prag.GetKey(SDL_Keycode.SDLK_RIGHT))
                            {
                                prag.playerVelocityGoal.x = 15f;
                            }
                            for (int i = 0; i < prag.Objects.Length; i++)
                            {
                                if (events.key.keysym.sym == SDL_Keycode.SDLK_SPACE && SDL_IntersectRect(ref prag.playerRect, ref prag.Objects[0], out prag.colliosion) == SDL_bool.SDL_TRUE)
                                {
                                    prag.playerVelocityGoal.y = -50f;
                                    prag.jumpS = true;
                                }
                            }

                            break;
                        case SDL_EventType.SDL_KEYUP:
                            if (SDL_Keycode.SDLK_LEFT == events.key.keysym.sym)
                            {
                                prag.playerVelocityGoal.x = 0f;
                            }
                            if (SDL_Keycode.SDLK_RIGHT == events.key.keysym.sym)
                            {
                                prag.playerVelocityGoal.x = 0f;
                                prag.run = false;
                            }
                            if (SDL_Keycode.SDLK_SPACE == events.key.keysym.sym)
                            {
                                prag.jumpS = false;
                                prag.playerVelocityGoal.y = 0f;
                            }
                            break;
                        default:
                            break;
                    }
                }
                frameTime++;
                if (FPS / frameTime == 4)
                {
                    prag.playerSrect.x += prag.PLAYER_WIDTH;
                    frameTime = 0;
                    if (prag.playerSrect.x >= prag.TEXTURE_WIDTH)
                    {
                        prag.playerSrect.x = 0;
                    }
                }
                prag.update();
                SDL_Delay(20);
                prag.present();
            }

            return 0;
        }
        bool GetKey(SDL.SDL_Keycode _keycode)
        {
            int arraySize;
            bool isKeyPressed = false;
            IntPtr origArray = SDL.SDL_GetKeyboardState(out arraySize);
            byte[] keys = new byte[arraySize];
            byte keycode = (byte)SDL.SDL_GetScancodeFromKey(_keycode);
            Marshal.Copy(origArray, keys, 0, arraySize);
            isKeyPressed = keys[keycode] == 1;
            return isKeyPressed;
        }
        
        void update()
        {

            SDL_SetRenderTarget(renderer, temp_floorTexture);
            SDL_SetRenderDrawColor(renderer, 3, 121, 111, 255);
            SDL_RenderClear(renderer);

            //player.move(playerVelocityGoal.x);

            //adding gravity

            for (int i = 0; i < Objects.Length; i++)
            {
                if (SDL_IntersectRect(ref playerRect, ref Objects[0], out colliosion) != SDL_bool.SDL_TRUE || jumpS)
                {
                    playerRect.y = (int)(playerRect.y + playerVelocity.Y * deltaTime);
                    //player.Trnasform = (int)(playerRect.y + playerVelocity.Y * deltaTime);
                }
            }


            if (playerVelocityGoal.y < 0)
            {
                playerVelocity.Y = playerVelocityGoal.y + gravity * deltaTime;
                //player.Velocity = playerVelocityGoal.y + gravity * deltaTime;
            }
            else if (playerVelocityGoal.y == 0)
            {
                playerVelocity.Y = playerVelocity.Y + gravity * deltaTime;
                //player.Velocity = player.Velocity + gravity * deltaTime;
            }



            SDL_SetRenderTarget(renderer, IntPtr.Zero);
            SDL_SetRenderDrawColor(renderer, 0, 0, 255, 255);
            SDL_RenderDrawRect(renderer, ref playerRect);
            SDL_RenderClear(renderer);
        }
        void present()
        {
            SDL_RenderClear(renderer);
            SDL_RenderCopy(renderer, backgroundtexture, IntPtr.Zero, IntPtr.Zero);
            if (run)
            {
                SDL_RenderCopy(renderer, playerTextureRun, ref playerSrect, ref playerRect);
            }
            else
            {
                SDL_RenderCopy(renderer, playerTextureIdle, ref playerSrect, ref playerRect);
            }

            SDL_RenderCopy(renderer, temp_floorTexture, IntPtr.Zero, ref Objects[0]);
            SDL_RenderCopy(renderer, temp_floorTexture, IntPtr.Zero, ref Objects[1]);
            player.Rendercpy(renderer);
            SDL_RenderPresent(renderer);
        }
    }

    class EntitySystem : Program
    {
        int ENTITY_WIDTH = 10;
        int ENTITY_HEIGHT = 10;
        int texturesCount = 0;
        nint texture;
        
        SDL_Rect transform;
        SDL_Rect textureTransform;
        Vector2 velocity = new Vector2(0, 0);
        public EntitySystem()
        {
            transform.x = 5;
            transform.y = 5;
            transform.w = ENTITY_WIDTH;
            transform.h = ENTITY_HEIGHT;
            textureTransform.x = 0;
            textureTransform.y = 0;
            textureTransform.w = ENTITY_WIDTH;
            textureTransform.h = ENTITY_HEIGHT;

        }
        ~EntitySystem()
        {
                SDL_DestroyTexture(texture);
        }
        struct transfromScale
        {
            public int w;
            public int h;
        }

        transfromScale textureSize;

        public void setSize(int h, int w)
        {
            ENTITY_WIDTH = w;
            ENTITY_HEIGHT = h;
        }
        public void setPosition(int x, int y)
        {
            transform.x = x;
            transform.y = y;
        }
        float findDirection(float current, float goal)
        {
            float diff = goal - current;
            //if the difrences is higher than the deltaTime means he is going
            //right lerp (goes from start to pos to end pos smothlly) if its negative then left
            if (diff > deltaTime)
            {
                return (float)current + (deltaTime * targetFps);
            }
            if (diff < -deltaTime)
            {
                return (float)current - (deltaTime * targetFps);
            }

            return goal;
        }
        public void move(float VelocityGoal)
        {
            //claculating the velocity struct for x
            velocity.X = (float)findDirection(velocity.X, VelocityGoal);
            //fixing the velocity tothe deltaTime aka to be in sync with screen and be in the border of 60fps
            //if not then it would be alot faster because then it will be depnedent to the processing speed.
            transform.x += (int)(velocity.X * deltaTime * 2);
        }
        public void createTexture(nint renderer, nint surface)
        {
            int textureSlices;
            texture = SDL_CreateTextureFromSurface(renderer, surface);
            texturesCount++;
            SDL_QueryTexture(texture, out _, out _, out textureSize.w, out textureSize.h);
            textureSlices = textureSize.w / 80;
            textureTransform.w = textureSize.w / textureSlices;
            textureTransform.h = textureSize.h;
            transform.w = textureSize.w / textureSlices;
            transform.h = textureSize.h;
            textureSlices = 0;

        }

        public void Rendercpy(nint renderer)
        {
            SDL_RenderCopy(renderer, texture, ref textureTransform, ref transform);
        }
    }
}