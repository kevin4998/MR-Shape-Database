package psb;

/**
 * <p>Title: Category</p>
 * <p>Description: </p>
 * <p>Copyright: Copyright (c) 2003</p>
 * <p>Company: </p>
 * @author Philip Shilane
 * @version 1.0
 */

public class Category {
   public String _name;
   public String _fullName;
   public String _parentCategory;
   public String[] _models;
   public int _position;  // num categories before, not number of models before
   public boolean _shouldShow;

   public Category(String name, String parent, int pos, boolean shouldShow) {
     _name = name;
     _parentCategory = parent;
     _position = pos;
     _shouldShow = shouldShow;
   }

   public String toString(){
     return _name + " child of " + _parentCategory + " (" + _fullName + ") " + _models.length + " models";
   }

}