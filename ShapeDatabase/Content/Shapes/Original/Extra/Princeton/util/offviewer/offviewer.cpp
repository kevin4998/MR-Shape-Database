// Source file for the OFF file viewer



////////////////////////////////////////////////////////////
// INCLUDE FILES
////////////////////////////////////////////////////////////

// Standard include files

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>
#include <math.h>
#include <assert.h>



// Windows include files 

#ifdef _WIN32
#include <windows.h>
#endif



// OpenGL include files 

#include <GL/gl.h>
#include <GL/glu.h>
#include <GL/glut.h>




////////////////////////////////////////////////////////////
// TYPE DEFINITIONS
////////////////////////////////////////////////////////////

typedef struct Vertex {
  float x, y, z;
} Vertex;

typedef struct Face {
  Face(void) : nverts(0), verts(0) {};
  int nverts;
  Vertex **verts;
  float normal[3];
} Face;

typedef struct Mesh {
  Mesh(void) : nverts(0), verts(0), nfaces(0), faces(0) {};
  int nverts;
  Vertex *verts;
  int nfaces;
  Face *faces;
} Mesh;





////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
////////////////////////////////////////////////////////////

// Program arguments

static char *filename = 0;



// GLUT variables 

static int GLUTwindow = 0;
static int GLUTwindow_height = 800;
static int GLUTwindow_width = 800;
static int GLUTmouse[2] = { 0, 0 };
static int GLUTbutton[3] = { 0, 0, 0 };
static int GLUTarrows[4] = { 0, 0, 0, 0 };
static int GLUTmodifiers = 0;



// Display variables

static int scaling = 0;
static int translating = 0;
static int rotating = 0;
static float scale = 1.0;
static float center[3] = { 0.0, 0.0, 0.0 };
static float rotation[3] = { 0.0, 0.0, 0.0 };
static float translation[3] = { 0.0, 0.0, -4.0 };



// Mesh variables

static Mesh *mesh = NULL;




////////////////////////////////////////////////////////////
// OFF FILE READING CODE
////////////////////////////////////////////////////////////

Mesh *
ReadOffFile(const char *filename)
{
  int i;

  // Open file
  FILE *fp;
  if (!(fp = fopen(filename, "r"))) {
    fprintf(stderr, "Unable to open file %s\n", filename);
    return 0;
  }

  // Allocate mesh structure
  Mesh *mesh = new Mesh();
  if (!mesh) {
    fprintf(stderr, "Unable to allocate memory for file %s\n", filename);
    fclose(fp);
    return 0;
  }

  // Read file
  int nverts = 0;
  int nfaces = 0;
  int nedges = 0;
  int line_count = 0;
  char buffer[1024];
  while (fgets(buffer, 1023, fp)) {
    // Increment line counter
    line_count++;

    // Skip white space
    char *bufferp = buffer;
    while (isspace(*bufferp)) bufferp++;

    // Skip blank lines and comments
    if (*bufferp == '#') continue;
    if (*bufferp == '\0') continue;

    // Check section
    if (nverts == 0) {
      // Read header 
      if (!strstr(bufferp, "OFF")) {
        // Read mesh counts
        if ((sscanf(bufferp, "%d%d%d", &nverts, &nfaces, &nedges) != 3) || (nverts == 0)) {
          fprintf(stderr, "Syntax error reading header on line %d in file %s\n", line_count, filename);
          fclose(fp);
          return NULL;
        }

        // Allocate memory for mesh
        mesh->verts = new Vertex [nverts];
        assert(mesh->verts);
        mesh->faces = new Face [nfaces];
        assert(mesh->faces);
      }
    }
    else if (mesh->nverts < nverts) {
      // Read vertex coordinates
      Vertex& vert = mesh->verts[mesh->nverts++];
      if (sscanf(bufferp, "%f%f%f", &(vert.x), &(vert.y), &(vert.z)) != 3) {
        fprintf(stderr, "Syntax error with vertex coordinates on line %d in file %s\n", line_count, filename);
        fclose(fp);
        return NULL;
      }
    }
    else if (mesh->nfaces < nfaces) {
      // Get next face
      Face& face = mesh->faces[mesh->nfaces++];

      // Read number of vertices in face 
      bufferp = strtok(bufferp, " \t");
      if (bufferp) face.nverts = atoi(bufferp);
      else {
        fprintf(stderr, "Syntax error with face on line %d in file %s\n", line_count, filename);
        fclose(fp);
        return NULL;
      }

      // Allocate memory for face vertices
      face.verts = new Vertex *[face.nverts];
      assert(face.verts);

      // Read vertex indices for face
      for (i = 0; i < face.nverts; i++) {
        bufferp = strtok(NULL, " \t");
        if (bufferp) face.verts[i] = &(mesh->verts[atoi(bufferp)]);
        else {
          fprintf(stderr, "Syntax error with face on line %d in file %s\n", line_count, filename);
          fclose(fp);
          return NULL;
        }
      }

      // Compute normal for face
      face.normal[0] = face.normal[1] = face.normal[2] = 0;
      Vertex *v1 = face.verts[face.nverts-1];
      for (i = 0; i < face.nverts; i++) {
        Vertex *v2 = face.verts[i];
        face.normal[0] += (v1->y - v2->y) * (v1->z + v2->z);
        face.normal[1] += (v1->z - v2->z) * (v1->x + v2->x);
        face.normal[2] += (v1->x - v2->x) * (v1->y + v2->y);
        v1 = v2;
      }

      // Normalize normal for face
      float squared_normal_length = 0.0;
      squared_normal_length += face.normal[0]*face.normal[0];
      squared_normal_length += face.normal[1]*face.normal[1];
      squared_normal_length += face.normal[2]*face.normal[2];
      float normal_length = sqrt(squared_normal_length);
      if (normal_length > 1.0E-6) {
        face.normal[0] /= normal_length;
        face.normal[1] /= normal_length;
        face.normal[2] /= normal_length;
      }
    }
    else {
      // Should never get here
      fprintf(stderr, "Found extra text starting at line %d in file %s\n", line_count, filename);
      break;
    }
  }

  // Check whether read all faces
  if (nfaces != mesh->nfaces) {
    fprintf(stderr, "Expected %d faces, but read only %d faces in file %s\n", nfaces, mesh->nfaces, filename);
  }

  // Close file
  fclose(fp);

  // Return mesh 
  return mesh;
}



////////////////////////////////////////////////////////////
// GLUT USER INTERFACE CODE
////////////////////////////////////////////////////////////

void GLUTRedraw(void)
{
  // Setup viewing transformation
  glLoadIdentity();
  glScalef(scale, scale, scale);
  glTranslatef(translation[0], translation[1], 0.0);

  // Set projection transformation
  glMatrixMode(GL_PROJECTION);
  glLoadIdentity();
  gluPerspective(45.0, (GLfloat) GLUTwindow_width /(GLfloat) GLUTwindow_height, 0.1, 100.0);

  // Set camera transformation
  glMatrixMode(GL_MODELVIEW);
  glLoadIdentity();
  glTranslatef(translation[0], translation[1], translation[2]);
  glScalef(scale, scale, scale);
  glRotatef(rotation[0], 1.0, 0.0, 0.0);
  glRotatef(rotation[1], 0.0, 1.0, 0.0);
  glRotatef(rotation[2], 0.0, 0.0, 1.0);
  glTranslatef(-center[0], -center[1], -center[2]);

  // Clear window 
  glClearColor(1.0, 1.0, 1.0, 1.0);
  glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

  // Set lights
  static GLfloat light0_position[] = { 3.0, 4.0, 5.0, 0.0 };
  glLightfv(GL_LIGHT0, GL_POSITION, light0_position);
  static GLfloat light1_position[] = { -3.0, -2.0, -3.0, 0.0 };
  glLightfv(GL_LIGHT1, GL_POSITION, light1_position);

  // Set material
  static GLfloat material[] = { 1.0, 0.5, 0.5, 1.0 };
  glMaterialfv(GL_FRONT, GL_AMBIENT_AND_DIFFUSE, material); 

  // Draw faces
  for (int i = 0; i < mesh->nfaces; i++) {
    Face& face = mesh->faces[i];
    glBegin(GL_POLYGON);
    glNormal3fv(face.normal);
    for (int j = 0; j < face.nverts; j++) {
      Vertex *vert = face.verts[j];
      glVertex3f(vert->x, vert->y, vert->z);
    }
    glEnd();
  }

  // Swap buffers 
  glutSwapBuffers();
}    



void GLUTStop(void)
{
  // Destroy window 
  glutDestroyWindow(GLUTwindow);

  // Exit
  exit(0);
}



void GLUTResize(int w, int h)
{
  // Resize window
  glViewport(0, 0, w, h);

  // Remember window size 
  GLUTwindow_width = w;
  GLUTwindow_height = h;

  // Redraw
  glutPostRedisplay();
}



void GLUTMotion(int x, int y)
{
  // Invert y coordinate
  y = GLUTwindow_height - y;
  
  // Process mouse motion event
  if (rotating) {
    // Rotate model
    rotation[0] += -0.5 * (y - GLUTmouse[1]);
    rotation[2] += 0.5 * (x - GLUTmouse[0]);
  }
  else if (scaling) {
    // Scale window
    scale *= exp(2.0 * (float) (x - GLUTmouse[0]) / (float) GLUTwindow_width);
  }
  else if (translating) {
    // Translate window
    translation[0] += 2.0 * (float) (x - GLUTmouse[0]) / (float) GLUTwindow_width;
    translation[1] += 2.0 * (float) (y - GLUTmouse[1]) / (float) GLUTwindow_height;
  }

  // Remember mouse position 
  GLUTmouse[0] = x;
  GLUTmouse[1] = y;
}



void GLUTMouse(int button, int state, int x, int y)
{
  // Invert y coordinate
  y = GLUTwindow_height - y;
  
  // Process mouse button event
  rotating = (button == GLUT_LEFT_BUTTON);
  scaling = (button == GLUT_MIDDLE_BUTTON);
  translating = (button == GLUT_RIGHT_BUTTON);
  if (rotating || scaling || translating) glutIdleFunc(GLUTRedraw);
  else glutIdleFunc(0);

  // Remember button state 
  int b = (button == GLUT_LEFT_BUTTON) ? 0 : ((button == GLUT_MIDDLE_BUTTON) ? 1 : 2);
  GLUTbutton[b] = (state == GLUT_DOWN) ? 1 : 0;

  // Remember modifiers 
  GLUTmodifiers = glutGetModifiers();

   // Remember mouse position 
  GLUTmouse[0] = x;
  GLUTmouse[1] = y;
}



void GLUTSpecial(int key, int x, int y)
{
  // Invert y coordinate
  y = GLUTwindow_height - y;

  // Process keyboard button event 

  // Remember mouse position 
  GLUTmouse[0] = x;
  GLUTmouse[1] = y;

  // Remember modifiers 
  GLUTmodifiers = glutGetModifiers();

  // Redraw
  glutPostRedisplay();
}



void GLUTKeyboard(unsigned char key, int x, int y)
{
  // Process keyboard button event 
  switch (key) {
  case 'Q':
  case 'q':
    GLUTStop();
    break;

  case 27: // ESCAPE
    GLUTStop();
    break;
  }

  // Remember mouse position 
  GLUTmouse[0] = x;
  GLUTmouse[1] = GLUTwindow_height - y;

  // Remember modifiers 
  GLUTmodifiers = glutGetModifiers();
}



void GLUTInit(int *argc, char **argv)
{
  // Open window 
  glutInit(argc, argv);
  glutInitWindowPosition(100, 100);
  glutInitWindowSize(GLUTwindow_width, GLUTwindow_height);
  glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH); // | GLUT_STENCIL
  GLUTwindow = glutCreateWindow("OpenGL Viewer");

  // Initialize GLUT callback functions 
  glutReshapeFunc(GLUTResize);
  glutDisplayFunc(GLUTRedraw);
  glutKeyboardFunc(GLUTKeyboard);
  glutSpecialFunc(GLUTSpecial);
  glutMouseFunc(GLUTMouse);
  glutMotionFunc(GLUTMotion);
  glutIdleFunc(0);
    
  // Initialize lights 
  static GLfloat lmodel_ambient[] = { 0.2, 0.2, 0.2, 1.0 };
  glLightModelfv(GL_LIGHT_MODEL_AMBIENT, lmodel_ambient);
  glLightModeli(GL_LIGHT_MODEL_LOCAL_VIEWER, GL_TRUE);
  static GLfloat light0_diffuse[] = { 1.0, 1.0, 1.0, 1.0 };
  glLightfv(GL_LIGHT0, GL_DIFFUSE, light0_diffuse);
  glEnable(GL_LIGHT0);
  static GLfloat light1_diffuse[] = { 0.5, 0.5, 0.5, 1.0 };
  glLightfv(GL_LIGHT1, GL_DIFFUSE, light1_diffuse);
  glEnable(GL_LIGHT1);
  glEnable(GL_NORMALIZE);
  glEnable(GL_LIGHTING);

  // Initialize graphics modes 
  glEnable(GL_DEPTH_TEST);
}



void GLUTMainLoop(void)
{
  // Compute bounding box
  float bbox[2][3] = { { 1.0E30F, 1.0E30F, 1.0E30F }, { -1.0E30F, -1.0E30F, -1.0E30F } };
  for (int i = 0; i < mesh->nverts; i++) {
    Vertex& vert = mesh->verts[i];
    if (vert.x < bbox[0][0]) bbox[0][0] = vert.x;
    else if (vert.x > bbox[1][0]) bbox[1][0] = vert.x;
    if (vert.y < bbox[0][1]) bbox[0][1] = vert.y;
    else if (vert.y > bbox[1][1]) bbox[1][1] = vert.y;
    if (vert.z < bbox[0][2]) bbox[0][2] = vert.z;
    else if (vert.z > bbox[1][2]) bbox[1][2] = vert.z;
  }

  // Setup initial viewing scale
  float dx = bbox[1][0] - bbox[0][0];
  float dy = bbox[1][1] - bbox[0][1];
  float dz = bbox[1][2] - bbox[0][2];
  scale = 2.0 / sqrt(dx*dx + dy*dy + dz*dz);

  // Setup initial viewing center
  center[0] = 0.5 * (bbox[1][0] + bbox[0][0]);
  center[1] = 0.5 * (bbox[1][1] + bbox[0][1]);
  center[2] = 0.5 * (bbox[1][2] + bbox[0][2]);

  // Run main loop -- never returns 
  glutMainLoop();
}





////////////////////////////////////////////////////////////
// PROGRAM ARGUMENT PARSING
////////////////////////////////////////////////////////////

int 
ParseArgs(int argc, char **argv)
{
  // Innocent until proven guilty
  int print_usage = 0;

  // Parse arguments
  argc--; argv++;
  while (argc > 0) {
    if ((*argv)[0] == '-') {
      if (!strcmp(*argv, "-help")) { print_usage = 1; }
      else { fprintf(stderr, "Invalid program argument: %s", *argv); exit(1); }
      argv++; argc--;
    }
    else {
      if (!filename) filename = *argv;
      else { fprintf(stderr, "Invalid program argument: %s", *argv); exit(1); }
      argv++; argc--;
    }
  }

  // Check filename
  if (!filename || print_usage) {
    printf("Usage: offviewer <filename>\n");
    return 0;
  }

  // Return OK status 
  return 1;
}



////////////////////////////////////////////////////////////
// MAIN
////////////////////////////////////////////////////////////

int 
main(int argc, char **argv)
{
  // Initialize GLUT
  GLUTInit(&argc, argv);

  // Parse program arguments
  if (!ParseArgs(argc, argv)) exit(1);

  // Read file
  mesh = ReadOffFile(filename);
  if (!mesh) exit(1);

  // Run GLUT interface
  GLUTMainLoop();

  // Return success 
  return 0;
}









