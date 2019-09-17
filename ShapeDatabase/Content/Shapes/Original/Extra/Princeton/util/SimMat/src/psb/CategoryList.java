package psb;

import java.io.*;
import java.util.*;

/**
 * <p>Title: CategoryList</p>
 * <p>Description: </p>
 * <p>Copyright: Copyright (c) 2003</p>
 * <p>Company: </p>
 * @author Philip Shilane
 * @version 1.0
 */

public class CategoryList {

  public static void main(String[] args) {
    CategoryList list = new CategoryList();
    list.readFile(args[0], true);
    System.out.println("printing all categories");
    list.printCategories();
  }

  public static final String CATEGORY_FILE_CODE = "PSB";
  public static final int FORMAT_NUM = 2;
  public static final String BASE_CATEGORY = "0";
  public static final String MISC = "-1";

  public ModelId[] _ids;
  public HashMap _models;
  public HashMap _categories;  // holds all categories including empty and non-empty
  public int _numModels;
  public Category[] _categoryOrder; // holds all non-empty categories
  public int _numCategories; // number of non-empty categories;

  public CategoryList() {
    _models = new HashMap();
    _categories = new HashMap();
    _numModels = 0;
    _numCategories = 0;
  }

  /* read the category file, returns true on success, false otherwise
  */
  public boolean readFile(String claFile, boolean verbose) {
    try{
      BufferedReader reader = new BufferedReader(new FileReader(claFile));

      if(verbose)System.out.println("Reading category file " + claFile);
        String line = reader.readLine();
        StringTokenizer st = new StringTokenizer(line);

      if (!CATEGORY_FILE_CODE.equals(st.nextToken())){
        System.err.println(claFile + " does not start with " + CATEGORY_FILE_CODE);
        reader.close();
        return false;
      }

      int version = Integer.parseInt(st.nextToken());
      if (FORMAT_NUM < version){
        System.err.println(claFile + " is not a supported format.  Supports format "
                           + FORMAT_NUM + " " + st.toString());
        reader.close();
        return false;
      }

      while((line = reader.readLine())!=null && line.trim().equals(""));
      st = new StringTokenizer(line);
      int numCategories = Integer.parseInt(st.nextToken());
      _categoryOrder = new Category[numCategories];

      int numModels = Integer.parseInt(st.nextToken());
      int currCatNum = 0;
      int pos = 0;

      ArrayList models = new ArrayList();

      while ((line = reader.readLine())!=null) {
        if (line.trim().equals(""))continue;
        if (currCatNum > numCategories) {
          System.err.println(claFile +
                             " has an incorrect format, too many categories. " +
                             st.toString());
          reader.close();
          return false;
        }
        st = new StringTokenizer(line);
        String category = st.nextToken();

        String parent = st.nextToken();

        Category catInfo = new Category(category, parent, currCatNum, true);
        String fullName = createFullName(category, parent, currCatNum);
        if (fullName == null){
          System.err.println(category + " has parent " + parent + " which does not exist.");
          reader.close();
          return false;
        }
        catInfo._fullName = fullName;

        int modelCount = Integer.parseInt(st.nextToken());

        if (modelCount > 0) {
          catInfo._models = new String[modelCount];
          for (int i = 0; i < modelCount; ++i) {

            String mid;
            while((mid = reader.readLine())!=null && mid.trim().equals(""));

            catInfo._models[i] = mid;

            ModelId modelInfo = new ModelId(mid, pos, category);
            models.add(modelInfo);
            _models.put(mid, modelInfo);
            ++pos;
          }
          _categoryOrder[currCatNum] = catInfo;
          ++currCatNum;
        }

        _categories.put(category, catInfo);
      }

      reader.close();

      Category[] temp = _categoryOrder;
      _categoryOrder = new Category[currCatNum];
      _numCategories = currCatNum;
      System.arraycopy(temp, 0, _categoryOrder, 0, currCatNum);

      _numModels = models.size();
      _ids = (ModelId[]) models.toArray(new ModelId[_numModels]);

      if (verbose) {
        System.out.println("Read " + _categoryOrder.length +
                           " non-empty categories, " + _numModels +
                           " model ids.");
      }
      return true;
    }catch (Exception ex) {
      ex.printStackTrace();
      return false;
    }


  }

  protected String createFullName(String category, String parent, int maxNumCategories){

    if (BASE_CATEGORY.equals(parent)){
        return category;
    }
    Category cat = (Category)_categories.get(parent);
    if (cat != null) return cat._fullName + "__" + category;
    return null;
  }

  public void printCategories(){
    for(int i = 0; i < _categoryOrder.length; ++i){
      System.out.println(_categoryOrder[i]);
    }

  }


}