package psb;

import javax.swing.*;
import java.awt.*;
import javax.imageio.*;
import java.awt.image.*;
import java.io.*;
import com.sun.image.codec.jpeg.*;
import java.util.*;
import java.awt.geom.AffineTransform;
import java.awt.geom.Rectangle2D;
import java.awt.font.FontRenderContext;
import Acme.JPM.Encoders.GifEncoder;


/**
 * <p>Title: Similarity Matrix</p>
 * <p>Description: </p>
 * <p>Copyright: Copyright (c) 2003</p>
 * <p>Company: </p>
 * @author Philip Shilane
 * @version 1.0
 *
 *
 *
 */

public class SimMat{


  public static void main(String[] args) {
    SimMat simMat1 = new SimMat(args);

  }



  protected static final Font DEFAULT_FONT = new Font("SansSerif", Font.PLAIN, 9);

  protected float[][] _matrix;
  protected double _totalValue;
  protected CategoryList _categoryList;
  protected boolean _color;
  protected String _outFile;
  protected BufferedImage _bufferedImage = null;
  protected int _maxStringLength;

// color settings
  protected int _lineColor;
  protected int _bestMatchColor;
  protected Color _backgroundColor;
  protected int _firstTierColor;
  protected int _secondTierColor;
  //protected int _thirdTierColor;



  public SimMat(String args[]) {

    handleArgs(args);

    reorderMatrix();
    calculateMaxString(); // must be after reorderMatrix

    long beforeTime= System.currentTimeMillis();

    if (_color){
      createColorSimilarityMatrix();
    }else{
      createBWSimiliarityMatrix();
    }
    long afterTime = System.currentTimeMillis();
    System.out.println(((afterTime - beforeTime) / 1000f) + " seconds.");
    //System.out.println((Runtime.getRuntime().totalMemory() / 1024 / 1024) + "MB");

  }

  protected void handleArgs(String args[]){

    if (args.length < 3){
      printUsage();
    }

    String matrixFile, categoryFile;
    _outFile = matrixFile = categoryFile = null;
    _color = true;
    boolean white = false;
    for(int i = 0; i < args.length; ++i){
      if (args[i].equals("-distance")){
        _color = false;
      }else if (args[i].equals("-paper")){
        white = true;
      }else if (categoryFile == null){
        categoryFile = args[i];
      }else if (matrixFile == null){
        matrixFile = args[i];
      }else if (_outFile == null){
        _outFile = args[i];
      }else{
        printUsage();
      }
    }

    setColors(white);

    try{

      _categoryList = new CategoryList();
      if(!_categoryList.readFile(categoryFile, true)){
        System.err.println("Unable to read " + categoryFile);
        System.exit(1);
      }
      readMatrixFile(matrixFile);

    }catch(Exception ex){
      ex.printStackTrace();
      System.exit(1);
    }

  }

  protected void setColors(boolean white){
    if (white){
      _lineColor = rgbToInt(30, 30, 30);
      _backgroundColor = Color.white;
      _bestMatchColor = rgbToInt(0, 0, 0);
      _firstTierColor = rgbToInt(255, 0, 0);
      _secondTierColor = rgbToInt(0, 0, 255);
      //_thirdTierColor = rgbToInt(0, 255, 100);

    }else{
      _lineColor = rgbToInt(30, 144, 255); // DodgerBlue1
      _backgroundColor = Color.black;
      _bestMatchColor = rgbToInt(255, 255, 255);
      _firstTierColor = rgbToInt(255, 255, 128);
      _secondTierColor = rgbToInt(255, 128, 25);
      //_thirdTierColor = rgbToInt(255, 0, 0);
    }
  }

  protected void printUsage(){
    System.out.println("java simMat categoryFile matrixFile imageName [-distance] [-screen]");
    System.out.println("The categoryFile is in PSB CLA format");
    System.out.println("The matrixFile is a file of floating point distance values in binary format.");
    System.out.println("The imageName will be the name of the created image.  ");
    System.out.println("    .jpg will be appended if necessary.");
    System.out.println("-distance specifies that a black and white image of the distance");
    System.out.println("    image should be created.  The default is to create a color");
    System.out.println("    image indicating the tier recall order.");
    System.out.println("-screen specifies that for color images, the background should be");
    System.out.println("    white instead of black, and the color scheme should be adjusted");

    System.exit(2);
  }



  /*
    matrix file is a binary file that consists of a symmetric
    dissimilarity matrix of floats that should be the size of the square
    of the number of ids since it represents the distance from
    each model to every other model.  A value of zero means the
    models are equivalent under this particular distance metric.
  */
  protected void readMatrixFile(String matrixFile) throws Exception{

    System.out.println("Reading matrix file " + matrixFile);
    DataInputStream reader = new DataInputStream(new BufferedInputStream(new FileInputStream(matrixFile)));

    _matrix = new float[_categoryList._numModels][_categoryList._numModels];
    byte[] allData = new byte[_categoryList._numModels*_categoryList._numModels*4];
    reader.readFully(allData);

    int start = 0;
    _totalValue = 0;
    for(int i = 0; i < _categoryList._numModels; i++){
      for(int j = 0; j < _categoryList._numModels; j++){

        // convert from bytes to floats
        int intValue = (allData[start] & 0xff) | ((allData[start+1] &0xff) << 8) | ((allData[start+2] &0xff) << 16) | ((allData[start + 3] &0xff)<< 24);
        float fltValue = Float.intBitsToFloat(intValue);
        _matrix[i][j] = fltValue;
        _totalValue += fltValue;

        start += 4;
      }
    }

    reader.close();
    System.out.println("Read matrix file, total size " + _categoryList._numModels * _categoryList._numModels + " entries.");

  }


  /** reorder the matrix  so the matrix is symmetric about the
      the line from bottom left to top right
  */

  protected void reorderMatrix(){

    float[][] copyMatrix = copyMatrix(_matrix, _categoryList._numModels, _categoryList._numModels);

    int idPos = 0;

    for(int i = 0; i < _categoryList._numModels; ++i){
      for(int j = 0; j < _categoryList._numModels; ++j){
        // replace _size - i - 1 with i, to change the line of symmetry
        copyMatrix[_categoryList._numModels - i - 1][j] = _matrix[_categoryList._ids[i]._position][_categoryList._ids[j]._position];
      }
    }

    _matrix = copyMatrix;


  }


  protected float[][] copyMatrix(float[][] orig, int rows, int cols){
    float[][] copy = new float[rows][cols];
    for(int i = 0; i < rows; ++i){
      System.arraycopy(orig[i], 0, copy[i], 0, cols);
    }
    return copy;
  }

  /**
     Writes out the similarity matrix in a black and white format, where the
     darker squares mean more similar.  first convert to similarity instead
     of dissimilarity
  */
  protected void createBWSimiliarityMatrix(){

    System.out.println("Creating black and white image.");

    int numCategories = _categoryList._numCategories;

    int size = (_categoryList._numModels*2 + numCategories - 1);

    _bufferedImage = new BufferedImage(size, size, BufferedImage.TYPE_BYTE_GRAY);

    //scale value so the average pixel is gray
    float scale = 0;
    scale = (float)(255 / 2.0 * (float)_categoryList._numModels * _categoryList._numModels / (double)_totalValue);

    float mean =(float)(_totalValue / (float)(size * size));

    int currRow = 0;
    int currCol = 0;
    int matrixRow = 0;
    int matrixCol = 0;

    int i = 0;
    int outerCategory = 0;
    while(outerCategory < numCategories){
      int outerCategorySize = _categoryList._categoryOrder[numCategories - outerCategory - 1]._models.length;
      ++outerCategory;
      int currCategoryCount = 0;

      while(i < _categoryList._numModels && currCategoryCount < outerCategorySize){
        currCol = 0;
        matrixCol = 0;
        int whichCategory = 0;

        while(whichCategory < numCategories){
          int categorySize = _categoryList._categoryOrder[whichCategory++]._models.length;

          for(int j = 0; j < categorySize; ++j){

            // if scale is not calculated, multiple by 255
            int v = (int)Math.min(scale * _matrix[matrixRow][matrixCol], 255);

            // from definition of Color
            int value = ((255 & 0xFF) << 24) | ((v & 0xFF) << 16) |
                ((v & 0xFF) << 8) | ((v & 0xFF) << 0);

            _bufferedImage.setRGB(currRow, currCol, value);
            _bufferedImage.setRGB(currRow, currCol+1, value);
            _bufferedImage.setRGB(currRow+1, currCol, value);
            _bufferedImage.setRGB(currRow+1, currCol+1, value);
            ++matrixCol;
            currCol +=2;
          }
          ++currCol; // dividing line
        }
        ++matrixRow;
        currRow += 2;
        ++i;
        ++currCategoryCount;
      }
      ++currRow; // dividing line

    }


    saveAsGIF(_outFile);

    System.out.println("Finished creating black and white image.");
  }


  /** converts red, green, and blue values in the range of 0-255 into
   *  a single int value used in BufferedImages.
   *
   * This implementation was copied from java.awt.Color
   */

  protected int rgbToInt(int r, int g, int b){
    return ((255 & 0xFF) << 24) |
                ((r & 0xFF) << 16) |
                ((g & 0xFF) << 8)  |
                ((b & 0xFF) << 0);
  }


  /*
    Writes out a similarity matrix where the colors represent
    black = query object and nearest neighbor
    red = top C - 2 matches where C is the size of the current category
    blue = next C - 1 matches where C is the size of the current category
  */
  protected void createColorSimilarityMatrix(){

    System.out.println("Creating color image.");
    int numCategories = _categoryList._numCategories;

    int size = (_categoryList._numModels*2 + numCategories - 1);

    _bufferedImage = new BufferedImage(size, size, BufferedImage.TYPE_3BYTE_BGR);

    Graphics g = _bufferedImage.getGraphics();
    g.setColor(_backgroundColor);
    g.fillRect(0, 0, size, size);

    int currRow = 0;

    // draw lines
    for(int i = 0; i < _categoryList._numCategories-1; ++i){
      int outerCategorySize = _categoryList._categoryOrder[i]._models.length;
      currRow += outerCategorySize * 2;
      for(int j = 0; j < size; ++j){
        _bufferedImage.setRGB(j, currRow, _lineColor);
        _bufferedImage.setRGB(size - currRow - 1, j, _lineColor);
      }
      ++currRow;
    }

    DataDistance[] data = new DataDistance[_categoryList._numModels];
    for(int i = 0; i < _categoryList._numModels; ++i){
      data[i] = new DataDistance();
    }
    CompareDistance cmpDistance = new CompareDistance();

    int categoryOffset = -1;
    String currentCategory = null;
    int classSize = 0;
    for(int i = 0; i < _categoryList._numModels; ++i){
      String category = _categoryList._ids[i]._category;
      if (currentCategory != category){
        categoryOffset++;
        currentCategory = category;
        Category catInfo = (Category)_categoryList._categories.get(category);
        classSize = catInfo._models.length;
      }
      if (category.equals(CategoryList.MISC)){
        continue;
      }

      for(int j = 0; j < _categoryList._numModels; ++j){
        data[j]._distance = _matrix[_categoryList._numModels - i - 1][j];
        data[j]._modelId = _categoryList._ids[j]._mid;
      }

      Arrays.sort(data, cmpDistance);

      int r = size - (2*i) - categoryOffset - 2;

      ModelId info = (ModelId)_categoryList._models.get(data[0]._modelId);
      int c = 2 * info._position;
      int otherOffset = ((Category)_categoryList._categories.get(info._category))._position;
      c += otherOffset;
      _bufferedImage.setRGB(r, c, _bestMatchColor);
      _bufferedImage.setRGB(r, c+1, _bestMatchColor);
      _bufferedImage.setRGB(r+1, c, _bestMatchColor);
      _bufferedImage.setRGB(r+1, c+1, _bestMatchColor);

      for(int k = 1; k < classSize; ++k){
        info = (ModelId)_categoryList._models.get(data[k]._modelId);
        c = 2 * info._position;
        otherOffset = ((Category)_categoryList._categories.get(info._category))._position;
        c += otherOffset;
        _bufferedImage.setRGB(r, c, _firstTierColor);
        _bufferedImage.setRGB(r, c+1, _firstTierColor);
        _bufferedImage.setRGB(r+1, c, _firstTierColor);
        _bufferedImage.setRGB(r+1, c+1, _firstTierColor);
      }

      for(int k = classSize; k < 2 * classSize - 1 && k < _categoryList._numModels; ++k){
        info = (ModelId)_categoryList._models.get(data[k]._modelId);
        c = 2 * info._position;
        otherOffset = ((Category)_categoryList._categories.get(info._category))._position;
        c += otherOffset;
        _bufferedImage.setRGB(r, c, _secondTierColor);
        _bufferedImage.setRGB(r, c+1, _secondTierColor);
        _bufferedImage.setRGB(r+1, c, _secondTierColor);
        _bufferedImage.setRGB(r+1, c+1, _secondTierColor);

      }

/*
       // for another tier of matches
       for(int k = 2 * classSize; k < 3 * classSize - 1 && k < _categoryList._numModels; ++k){
        info = (ModelId)_categoryList._models.get(data[k]._modelId);
        c = 2 * info._position;
        otherOffset = ((Category)_categoryList._categories.get(info._category))._position;
        c += otherOffset;
        _bufferedImage.setRGB(r, c, _thirdTierColor);
        _bufferedImage.setRGB(r, c+1, _thirdTierColor);
        _bufferedImage.setRGB(r+1, c, _thirdTierColor);
        _bufferedImage.setRGB(r+1, c+1, _thirdTierColor);
      }
*/
    }

    saveAsGIF(_outFile);

    System.out.println("Finished writing color image.");

  }


  protected void calculateMaxString(){

    _maxStringLength = 0;
    for(int i = 0; i < _categoryList._numCategories; ++i){
      String cat = _categoryList._categoryOrder[i]._name;
      _maxStringLength = Math.max(_maxStringLength, calculateStringSize(cat).width);
    }
  }


  protected int calculateFinalImageSize(){
    int numCategories = _categoryList._numCategories;
    int size = (_categoryList._numModels*2 + numCategories - 1);
    return size + _maxStringLength + 50;
  }

  // graphics stuff below here
  protected void printStrings(Graphics2D graphics){

    graphics.setColor(Color.white);
    int imageSize = calculateFinalImageSize();
    graphics.fillRect(0, 0, imageSize, imageSize);

    int offset = _maxStringLength + 8;
    AffineTransform af = AffineTransform.getTranslateInstance(offset, offset);

    AffineTransform normalTransform = graphics.getTransform();

    graphics.drawImage(_bufferedImage, af, null);
    graphics.setColor(Color.black);
    graphics.setFont(DEFAULT_FONT);

    int forwardSkipSize = offset + 10;
    int backSkipSize = offset + 5;
    int numCategories = _categoryList._numCategories;
    for(int i = 0; i < numCategories; ++i){
      String category = _categoryList._categoryOrder[i]._name;
      int size = calculateStringSize(category).width;
      graphics.drawString(category, _maxStringLength - size-2, forwardSkipSize);
      forwardSkipSize += ((Category)_categoryList._categories.get(category))._models.length * 2 + 1;

      graphics.rotate(Math.PI/2);

      String reverseCategory = _categoryList._categoryOrder[numCategories - i - 1]._name;
      size = calculateStringSize(reverseCategory).width;

      graphics.drawString(reverseCategory, (_maxStringLength - size -2), -backSkipSize);
      backSkipSize += ((Category)_categoryList._categories.get(reverseCategory))._models.length * 2 + 1;

      graphics.setTransform(normalTransform);
    }

  }



  /** Creates a new instance of saveToGIF */
  protected void saveAsGIF(String filename){
    try {
      if (_bufferedImage == null) return;

      int imageSize = calculateFinalImageSize();
      BufferedImage bimage = new BufferedImage(imageSize, imageSize, BufferedImage.TYPE_INT_RGB);
      printStrings(bimage.createGraphics());

      if(!filename.endsWith(".gif"))
        filename += ".gif";

      FileOutputStream fos = new FileOutputStream(filename);
      GifEncoder encoder = new GifEncoder(bimage, fos);
      encoder.encode();
      fos.close();

    }catch(Exception ex){
      System.out.println("trouble saving as gif");
      ex.printStackTrace();
    }

  }



  protected Dimension calculateStringSize(String text){
    FontRenderContext frc = new FontRenderContext(null, true, true);
    Rectangle2D defaultSize = DEFAULT_FONT.getStringBounds(text, frc);
    Dimension orig = defaultSize.getBounds().getSize();
    return new Dimension((int)orig.getWidth() + 5, (int)orig.getHeight() + 5);
  }

  class DataDistance{
    public String _modelId;
    public float _distance;
  }

  class CompareDistance implements Comparator{
      public int compare(Object first, Object second){
      return ( ( (DataDistance ) first)._distance <
              ( (DataDistance ) second)._distance ? -1 : 1);
    }
    public boolean equals(Object obj){
      return obj instanceof CompareDistance;
    }
  }



}