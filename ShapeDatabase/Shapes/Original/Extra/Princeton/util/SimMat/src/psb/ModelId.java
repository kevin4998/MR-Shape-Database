package psb;

/**
 * <p>Title: Similarity Matrix</p>
 * <p>Description: </p>
 * <p>Copyright: Copyright (c) 2003</p>
 * <p>Company: </p>
 * @author Philip Shilane
 * @version 1.0
 */

public class ModelId {
  public ModelId(){_mid = null; _position = -1; _category = null;};
  public ModelId(String mid, int pos, String category){_mid = mid; _position = pos; _category = category;};
  public String toString(){
    return _position + " " + _mid + " " + _category;
  }

  public int _position;
  public String _category;
  public String _mid;

}