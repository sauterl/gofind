using System.Collections.Generic;

namespace Assets.Modules.CineastAPI {
    public interface FilterStrategy {

        /**
         * <summary>Filters some mmos out and returnes the filtered list</summary>
         */
        List<MultimediaObject> applyFilter(List<MultimediaObject> list);
    }
}