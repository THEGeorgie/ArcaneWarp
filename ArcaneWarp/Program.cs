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
        struct entityTesting
        {
            nint texture;
            SDL_Rect enityRect;
            SDL_Rect enitySrect;
            int type;
        }
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
        SDL_Rect playerSrect; //Not used
        SDL_Rect colliosion;
        Velocity playerVelocity;
        Velocity playerVelocityGoal;
        SDL_Rect playArea;

        SDL_Rect[] Objects = new SDL_Rect[]
        {
            new SDL_Rect{x = 0,y = 370 ,w = 700,h = 30},
        };

        //Textures
        nint playerTextureIdle; //Not used
        nint playerTextureRun; //Not used
        nint backgroundtexture; //Not used

        //Temp textures
        nint temp_playerTexture;
        nint temp_floorTexture;
        //Init
        int PLAYER_WIDTH = 50;
        int PLAYER_HEIGHT = 80;

        float deltaTime;
        float buferJump;

        bool jumpS = false;

        public Program()
        {
            SDL_Init(SDL_INIT_VIDEO);

            SDL_CreateWindowAndRenderer(WINDOW_WIDTH, WINDOW_HEIGHT, 0, out window, out renderer);
            deltaTime = (float)1 / targetFps;

            backgroundtexture = SDL_CreateTextureFromSurface(renderer, SDL_image.IMG_Load("Background.png"));
            //playerTextureIdle = SDL_CreateTextureFromSurface(renderer, SDL_image.IMG_Load("Character/Idle/Idle-Sheet.png"));
            //playerTextureRun = SDL_CreateTextureFromSurface(renderer, SDL_image.IMG_Load("Character/Run/Run-Sheet.png"));
            temp_playerTexture = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_RGBA8888, (int)SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, PLAYER_WIDTH, PLAYER_HEIGHT);
            temp_floorTexture = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_RGBA8888, (int)SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, 600, 30);

            playerRect.x = 5;
            playerRect.y = 5;
            playerRect.w = PLAYER_WIDTH;
            playerRect.h = PLAYER_HEIGHT;

            playerVelocity.x = 0f;
            playerVelocity.y = 0f;

            playArea.w = 620;
            playArea.h = 420;
            playArea.x = 0;
            playArea.y = 0;

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

            while (loop)
            {

                //if the delta time is higher than 0.15f for every frame the speed will be to high
                if (prag.deltaTime > 0.15f)
                {
                    prag.deltaTime = 0.15f;
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
                            for (int i =0; i < prag.Objects.Length; i++)
                            {
                                if (events.key.keysym.sym == SDL_Keycode.SDLK_SPACE && SDL_HasIntersection(ref prag.playerRect, ref prag.Objects[i]) == SDL_bool.SDL_TRUE)
                                {
                                    prag.playerVelocityGoal.y = -300f;
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
        float move(float current, float goal)
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
        void update()
        {
            SDL_RenderDrawRect(renderer, ref playArea);
            SDL_SetRenderDrawColor(renderer, 0, 0, 255, 255);

            SDL_SetRenderTarget(renderer, temp_playerTexture);
            SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255);
            SDL_RenderClear(renderer);

            SDL_SetRenderTarget(renderer, temp_floorTexture);
            SDL_SetRenderDrawColor(renderer, 3, 121, 111, 255);
            SDL_RenderClear(renderer);

            //claculating the velocity struct for x
            playerVelocity.x = (float)move(playerVelocity.x, playerVelocityGoal.x);
            //fixing the velocity tothe deltaTime aka to be in sync with screen and be in the border of 60fps
            //if not then it would be alot faster because then it will be depnedent to the processing speed.
            playerRect.x += (int)(playerVelocity.x * deltaTime * 20);

            //adding gravity
            
            if (SDL_IntersectRect(ref playerRect, ref Objects[0], out colliosion) != SDL_bool.SDL_TRUE && colliosion.y != 1 || jumpS)
            {
                playerRect.y = (int)(playerRect.y + playerVelocity.y * deltaTime);
            }

            if (playerVelocityGoal.y < 0)
            {
                playerVelocity.y = playerVelocityGoal.y + gravity * deltaTime * 20;
            }
            else if (playerVelocityGoal.y == 0)
            {
                playerVelocity.y = playerVelocity.y + gravity * deltaTime * 20;
            }
            
            

            SDL_SetRenderTarget(renderer, IntPtr.Zero);
            SDL_SetRenderDrawColor(renderer, 0, 40, 255, 255);
            SDL_RenderFillRect(renderer, ref playArea);
            SDL_RenderClear(renderer);
        }
        void present()
        {
            SDL_RenderClear(renderer);
            //SDL_RenderCopy(renderer, backgroundtexture, IntPtr.Zero, IntPtr.Zero);
            SDL_RenderCopy(renderer, temp_playerTexture, IntPtr.Zero, ref playerRect);
            SDL_RenderCopy(renderer, temp_floorTexture, IntPtr.Zero, ref Objects[0]);
            SDL_RenderPresent(renderer);
        }
    }

    class EntitySystem
    {

    }
}