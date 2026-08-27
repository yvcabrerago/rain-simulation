using Raylib_cs;

// dimensions harcoded to background.png !
const int WIDTH = 400;
const int HEIGHT = 300;
const float DROPLET_FORCE = 10f;
const float DROPLET_RADIUS = 10;
const float GRADIENT_SCALE = 3f;
const int DECAY_FACTOR = 256;

float[] previousBuffer = new float[WIDTH * HEIGHT];
float[] currentBuffer = new float[WIDTH * HEIGHT];

Raylib.InitWindow(WIDTH, HEIGHT, "rain simulation");

Image image = Raylib.LoadImage("background.png");
Color[] frame = new Color[WIDTH * HEIGHT];
Color[] refcolors = new Color[WIDTH * HEIGHT];
unsafe
{
    Raylib.ImageFormat(&image, PixelFormat.UncompressedR8G8B8A8);
    Color* ptr = Raylib.LoadImageColors(image);
    for (int i = 0; i < WIDTH * HEIGHT; i++)
    {
        refcolors[i] = ptr[i];
    }

    Raylib.UnloadImageColors(ptr);
}

Texture2D texture = Raylib.LoadTextureFromImage(image);
Raylib.SetTargetFPS(60);
float elapsedTime = 0f;
float lastTime = 1f;

while (!Raylib.WindowShouldClose())
{
    elapsedTime += Raylib.GetFrameTime();
    if (elapsedTime >= lastTime)
    {
        elapsedTime = 0f;
        lastTime = Random.Shared.NextSingle() / 4;
        var mx = Random.Shared.Next(WIDTH);
        var my = Random.Shared.Next(HEIGHT);

        for (float dx = Math.Max(0, mx - DROPLET_RADIUS); dx < Math.Min(WIDTH, mx + DROPLET_RADIUS); dx++)
        {
            for (float dy = Math.Max(0, my - DROPLET_RADIUS); dy < Math.Min(HEIGHT, my + DROPLET_RADIUS); dy++)
            {
                var distance = Math.Abs(mx - dx) + Math.Abs(my - dy);
                if (distance > DROPLET_RADIUS) { continue; }
                currentBuffer[(int)dx + (int)dy * WIDTH] += DROPLET_FORCE * (1 - distance / DROPLET_RADIUS);
            }
        }
    }

    // wave
    for (int x = 1; x <= WIDTH - 2; x++)
    {
        for (int y = 1; y <= HEIGHT - 2; y++)
        {
            var index = x + y * WIDTH;
            var index1 = x - 1 + y * WIDTH;
            var index2 = x + 1 + y * WIDTH;
            var index3 = x + (y - 1) * WIDTH;
            var index4 = x + (y + 1) * WIDTH;
            var previousSum = (previousBuffer[index1] + previousBuffer[index2] + previousBuffer[index3] + previousBuffer[index4]) / 2f;
            currentBuffer[index] = previousSum - currentBuffer[index];
            currentBuffer[index] -= currentBuffer[index] / DECAY_FACTOR;
        }
    }

    // color
    for (int x = 0; x < WIDTH; x++)
    {
        for (int y = 0; y < HEIGHT; y++)
        {
            // gradient
            var index = x + y * WIDTH;
            if (x == 0 || y == 0 || x == WIDTH - 1 || y == HEIGHT - 1)
            {
                frame[index] = refcolors[x + y * WIDTH];
                continue;
            }

            var index1 = x - 1 + y * WIDTH;
            var index2 = x + 1 + y * WIDTH;
            var index3 = x + (y - 1) * WIDTH;
            var index4 = x + (y + 1) * WIDTH;

            var gradX = currentBuffer[index2] - currentBuffer[index1];
            var gradY = currentBuffer[index4] - currentBuffer[index3];

            var sampleX = x + gradX * GRADIENT_SCALE;
            var sampleY = y + gradY * GRADIENT_SCALE;

            var finalX = Math.Clamp(sampleX, 0, WIDTH - 1);
            var finalY = Math.Clamp(sampleY, 0, HEIGHT - 1);

            frame[index] = refcolors[(int)finalX + (int)finalY * WIDTH];
        }
    }

    // swap
    (previousBuffer, currentBuffer) = (currentBuffer, previousBuffer);

    Raylib.UpdateTexture(texture, frame);

    // draw
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.White);
    Raylib.DrawTexture(texture, 0, 0, Color.White);
    Raylib.DrawFPS(10, 10);
    Raylib.EndDrawing();
}

Raylib.UnloadImage(image);
Raylib.UnloadTexture(texture);
Raylib.CloseWindow();