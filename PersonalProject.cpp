#define _CRT_SECURE_NO_WARNINGS
#include <windows.h>
#include <GL/gl.h>
#include <GL/glu.h>
#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#include <math.h>

#pragma comment(lib, "opengl32.lib")
#pragma comment(lib, "glu32.lib")

// Globals
HDC   g_hDC = nullptr;
HGLRC g_hRC = nullptr;
HWND  g_hWnd = nullptr;
bool  g_running = true;

float g_angle = 0.0f;
GLuint g_texture = 0;
GLuint fontBase = 0;

// Camera
float camX = 0.0f, camY = 3.0f, camZ = 15.0f;
float rotX = 0.0f, rotY = 0.0f;
POINT lastMouse = { 0,0 };
bool mouseInitialized = false;

// Forward declarations
LRESULT CALLBACK WndProc(HWND, UINT, WPARAM, LPARAM);
void SetupPixelFormat(HDC hDC);
void RenderScene();
void DrawOverlay();
void BuildFont();
void TexturedCube(float w, float h, float d);
void GenerateRoughTexture();
void DrawCar();

// WinMain
int APIENTRY WinMain(HINSTANCE hInstance, HINSTANCE, LPSTR, int nCmdShow)
{
    srand((unsigned int)time(NULL));

    WNDCLASS wc = { 0 };
    wc.style = CS_OWNDC;
    wc.lpfnWndProc = WndProc;
    wc.hInstance = hInstance;
    wc.hCursor = LoadCursor(NULL, IDC_ARROW);
    wc.lpszClassName = TEXT("CarCameraWindow");

    RegisterClass(&wc);

    g_hWnd = CreateWindow(
        TEXT("CarCameraWindow"),
        TEXT("Orrin Roquemore"),
        WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT,
        800, 800,
        NULL, NULL, hInstance, NULL);

    ShowWindow(g_hWnd, nCmdShow);
    UpdateWindow(g_hWnd);

    g_hDC = GetDC(g_hWnd);
    SetupPixelFormat(g_hDC);

    g_hRC = wglCreateContext(g_hDC);
    wglMakeCurrent(g_hDC, g_hRC);

    BuildFont();  // NEW — Win32 font for overlay text

    // OpenGL setup
    glClearColor(0, 0, 0, 1);
    glEnable(GL_DEPTH_TEST);

    // Lighting + shading
    glEnable(GL_LIGHTING);
    glEnable(GL_LIGHT0);
    glEnable(GL_COLOR_MATERIAL);
    glColorMaterial(GL_FRONT_AND_BACK, GL_AMBIENT_AND_DIFFUSE);
    glShadeModel(GL_SMOOTH);

    GLfloat light_pos[] = { 100.0f, 150.0f, -100.0f, 1.0f };
    glLightfv(GL_LIGHT0, GL_POSITION, light_pos);

    // Projection
    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();
    gluPerspective(100.0, 1.0, 0.1, 1000.0);

    // Generate procedural car texture
    GenerateRoughTexture();

    // Main loop
    MSG msg;
    while (g_running)
    {
        while (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE))
        {
            if (msg.message == WM_QUIT)
                g_running = false;

            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }

        RenderScene();
        SwapBuffers(g_hDC);
    }

    wglMakeCurrent(NULL, NULL);
    wglDeleteContext(g_hRC);
    ReleaseDC(g_hWnd, g_hDC);

    return 0;
}

// Window procedure
LRESULT CALLBACK WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch (msg)
    {
    case WM_KEYDOWN:
    {
        float speed = 0.3f;
        float yawRad = rotY * 3.14159265f / 180.0f;

        float forwardX = sinf(yawRad);
        float forwardZ = -cosf(yawRad);
        float rightX = cosf(yawRad);
        float rightZ = sinf(yawRad);

        if (wParam == 'W')
        {
            camX += forwardX * speed;
            camZ += forwardZ * speed;
        }
        else if (wParam == 'S')
        {
            camX -= forwardX * speed;
            camZ -= forwardZ * speed;
        }
        else if (wParam == 'A')
        {
            camX -= rightX * speed;
            camZ -= rightZ * speed;
        }
        else if (wParam == 'D')
        {
            camX += rightX * speed;
            camZ += rightZ * speed;
        }
        else if (wParam == VK_ESCAPE)
        {
            PostQuitMessage(0);
        }
        return 0;
    }

    case WM_MOUSEMOVE:
    {
        int x = LOWORD(lParam);
        int y = HIWORD(lParam);

        if (!mouseInitialized)
        {
            lastMouse.x = x;
            lastMouse.y = y;
            mouseInitialized = true;
        }

        int dx = x - lastMouse.x;
        int dy = y - lastMouse.y;

        rotY += dx * 0.2f;
        rotX += dy * 0.2f;

        if (rotX > 89.0f)  rotX = 89.0f;
        if (rotX < -89.0f) rotX = -89.0f;

        lastMouse.x = x;
        lastMouse.y = y;

        return 0;
    }

    case WM_DESTROY:
        PostQuitMessage(0);
        return 0;
    }
    return DefWindowProc(hWnd, msg, wParam, lParam);
}

// Pixel format
void SetupPixelFormat(HDC hDC)
{
    PIXELFORMATDESCRIPTOR pfd = { 0 };
    pfd.nSize = sizeof(PIXELFORMATDESCRIPTOR);
    pfd.nVersion = 1;
    pfd.dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER;
    pfd.iPixelType = PFD_TYPE_RGBA;
    pfd.cColorBits = 24;
    pfd.cDepthBits = 24;
    pfd.iLayerType = PFD_MAIN_PLANE;

    int pf = ChoosePixelFormat(hDC, &pfd);
    SetPixelFormat(hDC, pf, &pfd);
}

// Build Win32 bitmap font
void BuildFont()
{
    HFONT font;
    fontBase = glGenLists(96);

    font = CreateFont(
        -24, 0, 0, 0,
        FW_BOLD,
        FALSE, FALSE, FALSE,
        ANSI_CHARSET,
        OUT_TT_PRECIS,
        CLIP_DEFAULT_PRECIS,
        ANTIALIASED_QUALITY,
        FF_DONTCARE | DEFAULT_PITCH,
        TEXT("Arial"));

    SelectObject(g_hDC, font);
    wglUseFontBitmaps(g_hDC, 32, 96, fontBase);
}

// Procedural rough blue texture
void GenerateRoughTexture()
{
    const int SIZE = 256;
    unsigned char* data = new unsigned char[SIZE * SIZE * 3];

    for (int y = 0; y < SIZE; y++)
    {
        for (int x = 0; x < SIZE; x++)
        {
            int i = (y * SIZE + x) * 3;

            unsigned char noise = rand() % 180;

            data[i + 0] = noise / 2;
            data[i + 1] = noise / 2;
            data[i + 2] = noise + 50;
        }
    }

    glGenTextures(1, &g_texture);
    glBindTexture(GL_TEXTURE_2D, g_texture);

    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB,
        SIZE, SIZE, 0,
        GL_RGB, GL_UNSIGNED_BYTE, data);

    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

    delete[] data;
}

// Draw textured cube
void TexturedCube(float w, float h, float d)
{
    w /= 2; h /= 2; d /= 2;

    glBindTexture(GL_TEXTURE_2D, g_texture);
    glEnable(GL_TEXTURE_2D);

    glBegin(GL_QUADS);

    // FRONT
    glNormal3f(0, 0, 1);
    glTexCoord2f(0, 0); glVertex3f(-w, -h, d);
    glTexCoord2f(1, 0); glVertex3f(w, -h, d);
    glTexCoord2f(1, 1); glVertex3f(w, h, d);
    glTexCoord2f(0, 1); glVertex3f(-w, h, d);

    // BACK
    glNormal3f(0, 0, -1);
    glTexCoord2f(0, 0); glVertex3f(-w, -h, -d);
    glTexCoord2f(1, 0); glVertex3f(w, -h, -d);
    glTexCoord2f(1, 1); glVertex3f(w, h, -d);
    glTexCoord2f(0, 1); glVertex3f(-w, h, -d);

    // LEFT
    glNormal3f(-1, 0, 0);
    glTexCoord2f(0, 0); glVertex3f(-w, -h, -d);
    glTexCoord2f(1, 0); glVertex3f(-w, -h, d);
    glTexCoord2f(1, 1); glVertex3f(-w, h, d);
    glTexCoord2f(0, 1); glVertex3f(-w, h, -d);

    // RIGHT
    glNormal3f(1, 0, 0);
    glTexCoord2f(0, 0); glVertex3f(w, -h, -d);
    glTexCoord2f(1, 0); glVertex3f(w, -h, d);
    glTexCoord2f(1, 1); glVertex3f(w, h, d);
    glTexCoord2f(0, 1); glVertex3f(w, h, -d);

    // TOP
    glNormal3f(0, 1, 0);
    glTexCoord2f(0, 0); glVertex3f(-w, h, -d);
    glTexCoord2f(1, 0); glVertex3f(w, h, -d);
    glTexCoord2f(1, 1); glVertex3f(w, h, d);
    glTexCoord2f(0, 1); glVertex3f(-w, h, d);

    // BOTTOM
    glNormal3f(0, -1, 0);
    glTexCoord2f(0, 0); glVertex3f(-w, -h, -d);
    glTexCoord2f(1, 0); glVertex3f(w, -h, -d);
    glTexCoord2f(1, 1); glVertex3f(w, -h, d);
    glTexCoord2f(0, 1); glVertex3f(-w, -h, d);

    glEnd();

    glDisable(GL_TEXTURE_2D);
}

// Full cube car
void DrawCar()
{
    glColor3f(0.2f, 0.4f, 1.0f);

    // Body
    glPushMatrix();
    TexturedCube(4.0f, 1.0f, 2.0f);
    glPopMatrix();

    // Cabin
    glPushMatrix();
    glTranslatef(0.0f, 0.8f, 0.0f);
    TexturedCube(2.0f, 1.0f, 2.0f);
    glPopMatrix();

    // Wheels
    glColor3f(0, 0, 0);

    glPushMatrix();
    glTranslatef(-1.5f, -0.8f, 1.2f);
    TexturedCube(0.8f, 0.8f, 0.5f);
    glPopMatrix();

    glPushMatrix();
    glTranslatef(1.5f, -0.8f, 1.2f);
    TexturedCube(0.8f, 0.8f, 0.5f);
    glPopMatrix();

    glPushMatrix();
    glTranslatef(-1.5f, -0.8f, -1.2f);
    TexturedCube(0.8f, 0.8f, 0.5f);
    glPopMatrix();

    glPushMatrix();
    glTranslatef(1.5f, -0.8f, -1.2f);
    TexturedCube(0.8f, 0.8f, 0.5f);
    glPopMatrix();
}

// 2D overlay
void DrawOverlay()
{
    glMatrixMode(GL_PROJECTION);
    glPushMatrix();
    glLoadIdentity();
    glOrtho(0, 800, 800, 0, -1, 1);

    glMatrixMode(GL_MODELVIEW);
    glPushMatrix();
    glLoadIdentity();

    glDisable(GL_LIGHTING);
    glDisable(GL_DEPTH_TEST);

    glColor3f(0.2f, 1.0f, 0.2f);
    glRasterPos2i(20, 40);

    const char* text = "CAR CAMERA ACTIVE";

    glPushAttrib(GL_LIST_BIT);
    glListBase(fontBase - 32);
    glCallLists(strlen(text), GL_UNSIGNED_BYTE, text);
    glPopAttrib();

    glEnable(GL_DEPTH_TEST);
    glEnable(GL_LIGHTING);

    glPopMatrix();
    glMatrixMode(GL_PROJECTION);
    glPopMatrix();
    glMatrixMode(GL_MODELVIEW);
}

// Render scene
void RenderScene()
{
    g_angle += 0.5f;
    if (g_angle > 360.0f) g_angle -= 360.0f;

    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity();

    // Camera rotation: X then Y
    glRotatef(rotX, 1, 0, 0);
    glRotatef(rotY, 0, 1, 0);
    glTranslatef(-camX, -camY, -camZ);

    glEnable(GL_NORMALIZE);

    // MAIN CAR (front)
    glPushMatrix();
    glTranslatef(0.0f, 0.0f, 0.0f);
    glRotatef(g_angle, 0, 1, 0);
    DrawCar();
    glPopMatrix();

    // LEFT CAR (back left)
    glPushMatrix();
    glTranslatef(-4.0f, 0.0f, -6.0f);
    glRotatef(-g_angle * 0.7f, 0, 1, 0);
    DrawCar();
    glPopMatrix();

    // RIGHT CAR (back right)
    glPushMatrix();
    glTranslatef(4.0f, 0.0f, -6.0f);
    glRotatef(g_angle * 0.5f, 0, 1, 0);
    DrawCar();
    glPopMatrix();

    DrawOverlay();

    glFlush();
}
